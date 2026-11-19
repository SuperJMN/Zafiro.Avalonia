using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using CSharpFunctionalExtensions;
using ReactiveUI;
using Zafiro.Avalonia.Dialogs.Views;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Dialogs.Implementations;

/// <summary>
/// Shows dialogs on top of the main view, for single-view platforms (mobile, TV, browser).
/// </summary>
/// <remarks>
/// A dialog is modal for the keyboard as well: focus cannot move to the view behind it. When the dialog was opened
/// from the keyboard, focus moves into it, and once it closes focus returns to the element that opened it. Escape and
/// the system Back button (Android phones and TV remotes) dismiss the topmost dialog instead of reaching the view
/// behind it, which could navigate away or close the app.
/// </remarks>
public class AdornerDialog : IDialog, ICloseable
{
    private readonly Lazy<AdornerLayer> adornerLayerLazy;
    private readonly Stack<DialogEntry> dialogs = new();
    private TopLevel? guardedTopLevel;
    private NavigationDirection? lastArrow;
    private IDisposable? backSubscription;

    public AdornerDialog(Func<AdornerLayer> getAdornerLayer)
    {
        KeyboardFocus.TrackInput();
        adornerLayerLazy = new Lazy<AdornerLayer>(() => getAdornerLayer());
    }

    public void Close()
    {
        Dispatcher.UIThread.Post(() => Pop(true));
    }

    public void Dismiss()
    {
        Dispatcher.UIThread.Post(() => Pop(false));
    }

    private void Pop(bool result)
    {
        if (dialogs.Count == 0)
        {
            return;
        }

        var entry = dialogs.Pop();
        var returnFromKeyboard = KeyboardFocus.IsKeyboardInUse;
        entry.TitleSubscription.Dispose();
        adornerLayerLazy.Value.Children.Remove(entry.Dialog);

        if (dialogs.Count == 0)
        {
            StopGuardingFocus();
        }

        RestoreFocus(entry.PreviousFocus, returnFromKeyboard);
        entry.Completion.TrySetResult(result);
    }

    public async Task<bool> Show<TViewModel>(Maybe<TViewModel> viewModel, Maybe<IObservable<string>> title, Func<Maybe<TViewModel>, ICloseable, IEnumerable<IOption>> optionsFactory, Maybe<object> icon = default, DialogTone tone = DialogTone.Neutral, DialogSize size = DialogSize.Auto)
    {
        if (optionsFactory == null) throw new ArgumentNullException(nameof(optionsFactory));

        var showTask = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var completion = new TaskCompletionSource<bool>();
            var options = optionsFactory(viewModel, this);

            var sizeHint = DialogSizeCalculator.Resolve(size);

            var dialog = new DialogViewContainer
            {
                Content = new DialogControl()
                {
                    Content = viewModel.GetValueOrDefault(),
                    Options = options,
                    Icon = icon.GetValueOrDefault(),
                    Tone = tone,
                    SizeHint = sizeHint
                },
                Close = ReactiveCommand.Create(() => Dismiss()),
            };

            // Tab keeps cycling through the dialog instead of leaving it.
            KeyboardNavigation.SetTabNavigation(dialog, KeyboardNavigationMode.Cycle);

            // Update the title reactively while the dialog is visible
            var titleSubscription = title.GetValueOrDefault(Observable.Never<string>())
                .Subscribe(t => Dispatcher.UIThread.Post(() => dialog.Title = t ?? string.Empty));

            var adornerLayer = adornerLayerLazy.Value;
            var previousFocus = KeyboardFocus.GetFocusedElement(adornerLayer);
            var openedFromKeyboard = KeyboardFocus.IsKeyboardInUse;

            dialog[!Layoutable.HeightProperty] = adornerLayer.Parent!
                .GetObservable(Visual.BoundsProperty)
                .Select(rect => rect.Height)
                .ToBinding();

            dialog[!Layoutable.WidthProperty] = adornerLayer.Parent!
                .GetObservable(Visual.BoundsProperty)
                .Select(rect => rect.Width)
                .ToBinding();

            // Apply proportional constraints to inner content via ContentMaxWidth/ContentMaxHeight
            var parentBoundsObs = adornerLayer.Parent!.GetObservable(Visual.BoundsProperty);

            dialog[!DialogViewContainer.ContentMaxWidthProperty] = parentBoundsObs
                .Select(rect => DialogSizeCalculator.Calculate(sizeHint, rect.Width, rect.Height).MaxWidth)
                .ToBinding();

            dialog[!DialogViewContainer.ContentMaxHeightProperty] = parentBoundsObs
                .Select(rect => DialogSizeCalculator.Calculate(sizeHint, rect.Width, rect.Height).MaxHeight)
                .ToBinding();

            adornerLayer.Children.Add(dialog);

            // Automatically push WindowNotificationManager to the top
            var wnm = adornerLayer.Children.OfType<WindowNotificationManager>().FirstOrDefault();
            if (wnm != null)
            {
                adornerLayer.Children.Remove(wnm);
                adornerLayer.Children.Add(wnm);
            }

            dialogs.Push(new DialogEntry(dialog, titleSubscription, completion, previousFocus));
            GuardFocus(TopLevel.GetTopLevel(adornerLayer));

            if (openedFromKeyboard)
            {
                Dispatcher.UIThread.Post(() => FocusInto(dialog), DispatcherPriority.Loaded);
            }

            return completion.Task;
        });

        return showTask;
    }

    private static IInputElement? FindFocusTarget(DialogViewContainer dialog)
    {
        var dialogControl = dialog.Content as Visual;
        return KeyboardFocus.FindFirstFocusable(dialogControl) ?? KeyboardFocus.FindFirstFocusable(dialog);
    }

    private void FocusInto(DialogViewContainer dialog)
    {
        if (dialogs.Count == 0 || !ReferenceEquals(dialogs.Peek().Dialog, dialog))
        {
            return;
        }

        if (!KeyboardFocus.IsWithin(KeyboardFocus.GetFocusedElement(dialog), dialog))
        {
            KeyboardFocus.FocusFromKeyboard(FindFocusTarget(dialog));
        }
    }

    private static void RestoreFocus(IInputElement? previousFocus, bool fromKeyboard)
    {
        if (!KeyboardFocus.CanFocus(previousFocus))
        {
            return;
        }

        if (fromKeyboard)
        {
            KeyboardFocus.FocusFromKeyboard(previousFocus);
        }
        else
        {
            previousFocus!.Focus();
        }
    }

    private void GuardFocus(TopLevel? topLevel)
    {
        if (topLevel is null || ReferenceEquals(guardedTopLevel, topLevel))
        {
            return;
        }

        StopGuardingFocus();
        guardedTopLevel = topLevel;
        topLevel.AddHandler(InputElement.GettingFocusEvent, OnGettingFocus, handledEventsToo: true);
        topLevel.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
        topLevel.AddHandler(InputElement.KeyDownEvent, OnEscape);
        // A class handler runs before the handlers of the view behind the dialog, such as a frame going back.
        backSubscription = TopLevel.BackRequestedEvent.AddClassHandler<TopLevel>(OnBackRequested);
    }

    private void StopGuardingFocus()
    {
        guardedTopLevel?.RemoveHandler(InputElement.GettingFocusEvent, OnGettingFocus);
        guardedTopLevel?.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        guardedTopLevel?.RemoveHandler(InputElement.KeyDownEvent, OnEscape);
        backSubscription?.Dispose();
        backSubscription = null;
        guardedTopLevel = null;
    }

    // Keeps focus inside the topmost dialog, and in the popups (menus, drop-downs) it opens.
    private void OnGettingFocus(object? sender, FocusChangingEventArgs e)
    {
        if (dialogs.Count == 0 || e.NewFocusedElement is null)
        {
            return;
        }

        var dialog = dialogs.Peek().Dialog;
        if (KeyboardFocus.IsWithin(e.NewFocusedElement, dialog) || KeyboardFocus.IsInPopupOf(e.NewFocusedElement, dialog))
        {
            return;
        }

        var target = KeyboardFocus.IsWithin(e.OldFocusedElement, dialog)
            ? NextInDirection(dialog, e)
            : FindFocusTarget(dialog);
        if (target is null || !e.TrySetNewFocusedElement(target))
        {
            e.TryCancel();
        }
    }

    // Arrow keys look for the nearest element in that direction across the whole window, which may be behind the
    // dialog. Searching only inside the dialog keeps the arrows useful instead of leaving focus stuck.
    private IInputElement? NextInDirection(DialogViewContainer dialog, FocusChangingEventArgs e)
    {
        if (e.NavigationMethod != NavigationMethod.Directional || lastArrow is not { } direction)
        {
            return null;
        }

        var next = TopLevel.GetTopLevel(dialog)?.FocusManager?.FindNextElement(direction, new FindNextElementOptions
        {
            FocusedElement = e.OldFocusedElement,
            SearchRoot = dialog,
        });

        return KeyboardFocus.IsWithin(next, dialog) ? next : null;
    }

    private void OnEscape(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            DismissTopmost(e);
        }
    }

    private void OnBackRequested(TopLevel topLevel, RoutedEventArgs e)
    {
        if (ReferenceEquals(topLevel, guardedTopLevel))
        {
            DismissTopmost(e);
        }
    }

    private void DismissTopmost(RoutedEventArgs e)
    {
        if (e.Handled || dialogs.Count == 0)
        {
            return;
        }

        e.Handled = true;
        Dismiss();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        lastArrow = e.Key switch
        {
            Key.Up => NavigationDirection.Up,
            Key.Down => NavigationDirection.Down,
            Key.Left => NavigationDirection.Left,
            Key.Right => NavigationDirection.Right,
            _ => null,
        };
    }

    private sealed record DialogEntry(
        DialogViewContainer Dialog,
        IDisposable TitleSubscription,
        TaskCompletionSource<bool> Completion,
        IInputElement? PreviousFocus);
}