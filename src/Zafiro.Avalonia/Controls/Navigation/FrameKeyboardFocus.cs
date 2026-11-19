using Avalonia.Input;
using Avalonia.Threading;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Controls.Navigation;

/// <summary>
/// Keeps keyboard focus inside a <see cref="Frame" /> while the user navigates its pages with a keyboard, a TV remote or
/// a gamepad.
/// </summary>
/// <remarks>
/// <para>
/// Navigating replaces the view that had focus, so focus would be lost and the next arrow key would start from an
/// arbitrary place. When the user was moving around the frame with the keyboard, focus goes to the first element of a
/// new page, or back to the element they used on the page they return to.
/// </para>
/// <para>
/// Pages are told apart by the type of their content: a navigator may build a new view model when it goes back.
/// Pointer and touch users are left alone.
/// </para>
/// </remarks>
internal sealed class FrameKeyboardFocus(Frame frame, Func<Control?> contentHost, Func<Control?> header)
{
    private const int MaxVisits = 64;
    private static readonly TimeSpan RestoreWait = TimeSpan.FromSeconds(5);

    private readonly List<PageVisit> visits = [];
    private EventHandler? pendingRestore;

    static FrameKeyboardFocus()
    {
        KeyboardFocus.TrackInput();
    }

    public void OnDescendantGotFocus(FocusChangedEventArgs e)
    {
        if (e.Source is not IInputElement element)
        {
            return;
        }

        if (visits.Count > 0)
        {
            visits[^1].Focus = KeyboardFocus.Remember(element, frame) ?? visits[^1].Focus;
        }
    }

    public void OnContentChanged(object? content)
    {
        var memento = Visit(content?.GetType());

        if (!KeyboardFocus.IsKeyboardInUse || TopLevel.GetTopLevel(frame) is null || !IsFocusInFrameOrLost())
        {
            return;
        }

        Dispatcher.UIThread.Post(() => FocusPage(content, memento), DispatcherPriority.Loaded);
    }

    // When nothing has focus (at startup, or after the focused element went away) start from the page.
    public void OnTopLevelKeyDown(KeyEventArgs e)
    {
        if (e.Handled || !KeyboardFocus.IsNavigationKey(e) || !frame.IsEffectivelyVisible || !frame.IsEffectivelyEnabled)
        {
            return;
        }

        var current = KeyboardFocus.GetFocusedElement(frame);
        if (current is InputElement { IsEffectivelyVisible: true } and not TopLevel && KeyboardFocus.IsAttached(current))
        {
            return;
        }

        var remembered = visits.Count > 0 && visits[^1].Focus is { } memento ? KeyboardFocus.Recall(memento, frame) : null;
        if (KeyboardFocus.FocusFromKeyboard(remembered ?? FirstOnPage()))
        {
            e.Handled = true;
        }
    }

    // Returns what was focused on the page being shown again, or null for a page visited anew.
    private FocusMemento? Visit(Type? pageType)
    {
        if (visits.Count >= 2 && visits[^2].PageType == pageType)
        {
            visits.RemoveAt(visits.Count - 1);
            return visits[^1].Focus;
        }

        visits.Add(new PageVisit(pageType));
        if (visits.Count > MaxVisits)
        {
            visits.RemoveAt(0);
        }

        return null;
    }

    private void FocusPage(object? content, FocusMemento? memento)
    {
        if (!ReferenceEquals(frame.Content, content) || !IsFocusInFrameOrLost())
        {
            return;
        }

        var current = KeyboardFocus.GetFocusedElement(frame);
        if (contentHost() is { } host && KeyboardFocus.IsAttached(current) && KeyboardFocus.IsWithin(current, host))
        {
            return;
        }

        var remembered = memento is null ? null : KeyboardFocus.Recall(memento, frame);
        var target = remembered ?? FirstOnPage();
        KeyboardFocus.FocusFromKeyboard(target);

        if (remembered is null && memento is not null)
        {
            RestoreWhenShown(content, memento, KeyboardFocus.GetFocusedElement(frame));
        }
    }

    // A page often loads its items after it is shown. Until it does, focus waits on the first element and moves to the
    // remembered one once it appears, unless the user moved on.
    private void RestoreWhenShown(object? content, FocusMemento memento, IInputElement? placeholder)
    {
        StopWaiting();
        var deadline = DateTime.UtcNow + RestoreWait;

        pendingRestore = (_, _) =>
        {
            var current = KeyboardFocus.GetFocusedElement(frame);
            var stillWaiting = ReferenceEquals(frame.Content, content) && DateTime.UtcNow < deadline &&
                               (ReferenceEquals(current, placeholder) || current is null or TopLevel);
            if (!stillWaiting)
            {
                StopWaiting();
                return;
            }

            if (KeyboardFocus.Recall(memento, frame) is { } remembered && KeyboardFocus.FocusFromKeyboard(remembered))
            {
                StopWaiting();
            }
        };

        frame.LayoutUpdated += pendingRestore;
    }

    private void StopWaiting()
    {
        if (pendingRestore is not null)
        {
            frame.LayoutUpdated -= pendingRestore;
            pendingRestore = null;
        }
    }

    private IInputElement? FirstOnPage()
    {
        return KeyboardFocus.FindFirstFocusable(contentHost(), avoidTextInput: true)
               ?? KeyboardFocus.FindFirstFocusable(header(), avoidTextInput: true)
               ?? KeyboardFocus.FindFirstFocusable(frame, avoidTextInput: true);
    }

    private bool IsFocusInFrameOrLost()
    {
        var current = KeyboardFocus.GetFocusedElement(frame);
        return current is null or TopLevel || !KeyboardFocus.IsAttached(current) || KeyboardFocus.IsWithin(current, frame);
    }

    private sealed class PageVisit(Type? pageType)
    {
        public Type? PageType { get; } = pageType;

        public FocusMemento? Focus { get; set; }
    }
}
