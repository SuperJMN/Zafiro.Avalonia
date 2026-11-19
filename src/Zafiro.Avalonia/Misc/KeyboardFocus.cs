using System.Reactive.Linq;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Zafiro.Avalonia.Misc;

/// <summary>
/// Helpers that keep keyboard focus where people who navigate with a keyboard, a TV remote or a gamepad expect it.
/// </summary>
/// <remarks>
/// Focus only moves on its own while the keyboard is in use (see <see cref="IsKeyboardInUse" />), so pointer and touch
/// users are not affected by focus moves that only make sense for keyboards, such as opening the on-screen keyboard.
/// </remarks>
public static class KeyboardFocus
{
    private static bool keyboardInUse;
    private static Key? pressedKey;
    private static WeakReference<IInputElement>? pressedOn;

    static KeyboardFocus()
    {
        InputElement.KeyDownEvent.Raised.Select(_ => true)
            .Merge(InputElement.PointerPressedEvent.Raised.Select(_ => false))
            .Do(fromKeyboard => keyboardInUse = fromKeyboard)
            .Subscribe();

        InputElement.KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.KeyUpEvent.AddClassHandler<TopLevel>(OnKeyUp, RoutingStrategies.Tunnel);
    }

    // Pressing a key often moves focus: Space opens an item and focus goes into the page or dialog it shows. Buttons
    // click when Space is released, so the release would press whatever got focus in between, such as an action on the
    // new page. A release only reaches the element that saw the key go down.
    private static void OnKeyDown(TopLevel topLevel, KeyEventArgs e)
    {
        pressedKey = e.Key;
        pressedOn = topLevel.FocusManager?.GetFocusedElement() is { } focused ? new WeakReference<IInputElement>(focused) : null;
    }

    private static void OnKeyUp(TopLevel topLevel, KeyEventArgs e)
    {
        if (e.Key != Key.Space)
        {
            return;
        }

        var focused = topLevel.FocusManager?.GetFocusedElement();
        var pressedOnFocused = pressedKey == Key.Space && pressedOn is not null && pressedOn.TryGetTarget(out var target) && ReferenceEquals(target, focused);
        if (!pressedOnFocused)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Tells whether the last input came from a keyboard, a TV remote or a gamepad rather than from a pointer or touch.
    /// </summary>
    public static bool IsKeyboardInUse => keyboardInUse;

    /// <summary>
    /// Starts watching the input, so <see cref="IsKeyboardInUse" /> knows which device was used last. Call it before the
    /// first input arrives, for instance when creating a control that relies on <see cref="IsKeyboardInUse" />.
    /// </summary>
    public static void TrackInput()
    {
    }

    /// <summary>
    /// Gets the element that currently has keyboard focus in the top level that hosts <paramref name="visual" />.
    /// </summary>
    public static IInputElement? GetFocusedElement(Visual visual)
    {
        return TopLevel.GetTopLevel(visual)?.FocusManager?.GetFocusedElement();
    }

    /// <summary>
    /// Tells whether <paramref name="element" /> is <paramref name="scope" /> or lives inside it.
    /// </summary>
    public static bool IsWithin(IInputElement? element, Visual scope)
    {
        return element is Visual visual && (ReferenceEquals(visual, scope) || scope.IsVisualAncestorOf(visual));
    }

    /// <summary>
    /// Tells whether <paramref name="element" /> lives in a popup (flyout, menu, drop-down) opened from inside
    /// <paramref name="scope" />.
    /// </summary>
    public static bool IsInPopupOf(IInputElement? element, Visual scope)
    {
        if (element is not Visual visual || !visual.GetSelfAndVisualAncestors().Any(ancestor => ancestor is PopupRoot or OverlayPopupHost))
        {
            return false;
        }

        return visual is ILogical logical &&
               logical.GetSelfAndLogicalAncestors().OfType<Visual>().Any(ancestor => IsWithin(ancestor as IInputElement, scope));
    }

    /// <summary>
    /// Tells whether <paramref name="element" /> is still part of a visual tree that is shown.
    /// </summary>
    public static bool IsAttached(IInputElement? element)
    {
        return element is Visual visual && TopLevel.GetTopLevel(visual) is not null;
    }

    /// <summary>
    /// Tells whether <paramref name="element" /> can take keyboard focus right now.
    /// </summary>
    public static bool CanFocus(IInputElement? element)
    {
        return element is InputElement { Focusable: true, IsEffectivelyEnabled: true, IsEffectivelyVisible: true } inputElement &&
               IsAttached(inputElement);
    }

    /// <summary>
    /// Finds the first element inside <paramref name="scope" /> that takes focus, following the Tab order
    /// (<see cref="KeyboardNavigation.TabIndexProperty" /> first, then the order in the visual tree).
    /// </summary>
    /// <param name="scope">Where to look.</param>
    /// <param name="avoidTextInput">
    /// Prefers any other element over a text box. Focusing a text box opens the on-screen keyboard on mobile and TV,
    /// which gets in the way when focus moves there on its own rather than because the user wants to type.
    /// </param>
    public static IInputElement? FindFirstFocusable(Visual? scope, bool avoidTextInput = false)
    {
        if (scope is not { IsEffectivelyVisible: true } || TopLevel.GetTopLevel(scope) is null)
        {
            return null;
        }

        var candidates = Focusable(scope);

        return avoidTextInput
            ? candidates.FirstOrDefault(element => element is not TextBox) ?? candidates.FirstOrDefault()
            : candidates.FirstOrDefault();
    }

    private static IEnumerable<InputElement> InTabOrder(Visual visual)
    {
        var children = visual.GetVisualChildren()
            .Where(child => child.IsVisible)
            .OrderBy(child => child is InputElement element ? KeyboardNavigation.GetTabIndex(element) : int.MaxValue);

        foreach (var child in children)
        {
            if (child is InputElement element)
            {
                yield return element;
            }

            foreach (var descendant in InTabOrder(child))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Moves focus to <paramref name="element" /> as keyboard navigation does, so it shows its focus visual.
    /// </summary>
    public static bool FocusFromKeyboard(IInputElement? element)
    {
        return CanFocus(element) && element!.Focus(NavigationMethod.Directional);
    }

    /// <summary>
    /// Tells whether the key moves focus: arrows (which TV remotes and gamepads also send) and Tab.
    /// </summary>
    public static bool IsNavigationKey(KeyEventArgs e)
    {
        return e.Key switch
        {
            Key.Up or Key.Down or Key.Left or Key.Right => e.KeyModifiers == KeyModifiers.None,
            Key.Tab => e.KeyModifiers is KeyModifiers.None or KeyModifiers.Shift,
            _ => false,
        };
    }

    /// <summary>
    /// Remembers which element has focus inside <paramref name="scope" /> in a way that survives rebuilding the view:
    /// by its data context, and by its position in the Tab order for when the view models are rebuilt too.
    /// </summary>
    public static FocusMemento? Remember(IInputElement? focused, Visual scope)
    {
        if (focused is not InputElement element || !IsWithin(element, scope))
        {
            return null;
        }

        var dataContext = element.DataContext;
        var type = element.GetType();
        var sameContextIndex = SameContext(scope, dataContext, type).TakeWhile(candidate => !ReferenceEquals(candidate, element)).Count();
        var focusable = Focusable(scope);

        return new FocusMemento(dataContext, type, sameContextIndex, focusable.IndexOf(element), focusable.Count);
    }

    /// <summary>
    /// Finds the element inside <paramref name="scope" /> that matches a <see cref="FocusMemento" />, or null when the
    /// view does not show it (yet).
    /// </summary>
    public static IInputElement? Recall(FocusMemento memento, Visual scope)
    {
        if (memento.DataContext is not null)
        {
            var sameContext = SameContext(scope, memento.DataContext, memento.Type).ToList();
            if (sameContext.Count > 0)
            {
                return sameContext.ElementAtOrDefault(memento.SameContextIndex) ?? sameContext[0];
            }
        }

        // The view models were rebuilt: when the view has the same shape, the element at the same place is the one.
        var focusable = Focusable(scope);
        return focusable.Count == memento.FocusableCount && memento.TabPosition >= 0 && focusable[memento.TabPosition].GetType() == memento.Type
            ? focusable[memento.TabPosition]
            : null;
    }

    private static List<InputElement> Focusable(Visual scope)
    {
        return InTabOrder(scope).Where(element => CanFocus(element) && KeyboardNavigation.GetIsTabStop(element)).ToList();
    }

    private static IEnumerable<InputElement> SameContext(Visual scope, object? dataContext, Type type)
    {
        return scope.GetVisualDescendants()
            .OfType<InputElement>()
            .Where(element => element.GetType() == type && Equals(element.DataContext, dataContext) && CanFocus(element));
    }
}

/// <summary>
/// Identifies a focused element by what it shows rather than by instance. See <see cref="KeyboardFocus.Remember" />.
/// </summary>
public sealed record FocusMemento(object? DataContext, Type Type, int SameContextIndex, int TabPosition, int FocusableCount);
