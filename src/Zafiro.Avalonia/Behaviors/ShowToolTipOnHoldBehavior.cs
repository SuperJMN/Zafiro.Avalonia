using System.Reactive.Disposables;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using Avalonia.Xaml.Interactions.Custom;
using JetBrains.Annotations;

namespace Zafiro.Avalonia.Behaviors;

[PublicAPI]
public class ShowToolTipOnHoldBehavior : DisposingBehavior<Control>
{
    public static readonly StyledProperty<bool> EnableMouseHoldProperty =
        AvaloniaProperty.Register<ShowToolTipOnHoldBehavior, bool>(nameof(EnableMouseHold));

    public static readonly StyledProperty<bool> CancelActivationOnHoldProperty =
        AvaloniaProperty.Register<ShowToolTipOnHoldBehavior, bool>(nameof(CancelActivationOnHold), true);

    public static readonly StyledProperty<bool> CloseOnReleaseProperty =
        AvaloniaProperty.Register<ShowToolTipOnHoldBehavior, bool>(nameof(CloseOnRelease), true);

    private ClickMode? originalClickMode;
    private bool shouldCancelNextActivation;
    private bool wasHoldingEnabled;
    private bool wasHoldWithMouseEnabled;

    public bool EnableMouseHold
    {
        get => GetValue(EnableMouseHoldProperty);
        set => SetValue(EnableMouseHoldProperty, value);
    }

    public bool CancelActivationOnHold
    {
        get => GetValue(CancelActivationOnHoldProperty);
        set => SetValue(CancelActivationOnHoldProperty, value);
    }

    public bool CloseOnRelease
    {
        get => GetValue(CloseOnReleaseProperty);
        set => SetValue(CloseOnReleaseProperty, value);
    }

    protected override IDisposable OnAttachedOverride()
    {
        var disposables = new CompositeDisposable();

        if (AssociatedObject is null)
        {
            return disposables;
        }

        wasHoldingEnabled = InputElement.GetIsHoldingEnabled(AssociatedObject);
        wasHoldWithMouseEnabled = InputElement.GetIsHoldWithMouseEnabled(AssociatedObject);

        InputElement.SetIsHoldingEnabled(AssociatedObject, true);

        this.WhenAnyValue(x => x.EnableMouseHold)
            .Do(enable => InputElement.SetIsHoldWithMouseEnabled(AssociatedObject, enable))
            .Subscribe()
            .DisposeWith(disposables);

        AssociatedObject
            .AddDisposableHandler(InputElement.HoldingEvent, OnHolding, RoutingStrategies.Bubble)
            .DisposeWith(disposables);

        AssociatedObject
            .AddDisposableHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble, true)
            .DisposeWith(disposables);

        AssociatedObject
            .AddDisposableHandler(InputElement.TappedEvent, OnActivation, RoutingStrategies.Bubble, true)
            .DisposeWith(disposables);

        if (AssociatedObject is Button button)
        {
            button
                .AddDisposableHandler(Button.ClickEvent, OnActivation, RoutingStrategies.Bubble, true)
                .DisposeWith(disposables);
        }

        Disposable
            .Create(RestoreAssociatedState)
            .DisposeWith(disposables);

        return disposables;
    }

    private void OnHolding(object? sender, HoldingRoutedEventArgs e)
    {
        HandleHoldingState(e.HoldingState);
        e.Handled = true;
    }

    internal void HandleHoldingState(HoldingState holdingState)
    {
        switch (holdingState)
        {
            case HoldingState.Started:
                ShowToolTip();
                StartActivationCancellation();
                break;
            case HoldingState.Completed:
                CloseToolTip();
                RestoreClickMode();
                break;
            case HoldingState.Canceled:
                CloseToolTip();
                StopActivationCancellation();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(holdingState), holdingState, "Unsupported holding state.");
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!shouldCancelNextActivation)
        {
            return;
        }

        e.Handled = true;

        if (CloseOnRelease)
        {
            CloseToolTip();
        }

        Dispatcher.UIThread.Post(StopActivationCancellation, DispatcherPriority.Background);
    }

    private void OnActivation(object? sender, RoutedEventArgs e)
    {
        if (!shouldCancelNextActivation)
        {
            return;
        }

        e.Handled = true;
        if (CloseOnRelease)
        {
            CloseToolTip();
        }

        StopActivationCancellation();
    }

    private void ShowToolTip()
    {
        if (AssociatedObject is null || ToolTip.GetTip(AssociatedObject) is null)
        {
            return;
        }

        ToolTip.SetIsOpen(AssociatedObject, true);
    }

    private void CloseToolTip()
    {
        if (AssociatedObject is null)
        {
            return;
        }

        ToolTip.SetIsOpen(AssociatedObject, false);
    }

    private void StartActivationCancellation()
    {
        if (!CancelActivationOnHold || AssociatedObject is null || ToolTip.GetTip(AssociatedObject) is null)
        {
            return;
        }

        shouldCancelNextActivation = true;

        if (AssociatedObject is Button { ClickMode: ClickMode.Release } button && originalClickMode is null)
        {
            originalClickMode = button.ClickMode;
            button.SetCurrentValue(Button.ClickModeProperty, ClickMode.Press);
        }
    }

    private void StopActivationCancellation()
    {
        shouldCancelNextActivation = false;
        RestoreClickMode();
    }

    private void RestoreClickMode()
    {
        if (AssociatedObject is not Button button || originalClickMode is not { } clickMode)
        {
            return;
        }

        button.SetCurrentValue(Button.ClickModeProperty, clickMode);
        originalClickMode = null;
    }

    private void RestoreAssociatedState()
    {
        if (AssociatedObject is null)
        {
            return;
        }

        CloseToolTip();
        StopActivationCancellation();
        InputElement.SetIsHoldingEnabled(AssociatedObject, wasHoldingEnabled);
        InputElement.SetIsHoldWithMouseEnabled(AssociatedObject, wasHoldWithMouseEnabled);
    }
}
