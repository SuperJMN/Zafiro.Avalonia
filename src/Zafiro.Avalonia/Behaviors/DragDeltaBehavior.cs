using System.Reactive.Disposables;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.Custom;
using Zafiro.Avalonia.Misc;
using Zafiro.Reactive;

namespace Zafiro.Avalonia.Behaviors;

public class DragDeltaBehavior : AttachedToVisualTreeBehavior<Control>
{
    public static readonly StyledProperty<RoutingStrategies> RoutingStrategyProperty = AvaloniaProperty.Register<DragDeltaBehavior, RoutingStrategies>(nameof(RoutingStrategy), RoutingStrategies.Tunnel);

    public static readonly StyledProperty<double> LeftProperty = AvaloniaProperty.Register<DragDeltaBehavior, double>(nameof(Left), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> TopProperty = AvaloniaProperty.Register<DragDeltaBehavior, double>(nameof(Top), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<MouseButton> DragButtonProperty = AvaloniaProperty.Register<DragDeltaBehavior, MouseButton>(nameof(DragButton), MouseButton.Left);


    private Point? lastPosition;

    public RoutingStrategies RoutingStrategy
    {
        get => GetValue(RoutingStrategyProperty);
        set => SetValue(RoutingStrategyProperty, value);
    }

    public double Left
    {
        get => GetValue(LeftProperty);
        set => SetValue(LeftProperty, value);
    }

    public double Top
    {
        get => GetValue(TopProperty);
        set => SetValue(TopProperty, value);
    }

    public MouseButton DragButton
    {
        get => GetValue(DragButtonProperty);
        set => SetValue(DragButtonProperty, value);
    }

    protected override IDisposable OnAttachedToVisualTreeOverride()
    {
        var disposables = new CompositeDisposable();

        if (AssociatedObject is null)
        {
            return disposables;
        }

        // Use the first ancestor that is a Visual as a stable coordinate system
        var container = AssociatedObject.FindAncestorOfType<Visual>();
        if (container is null)
        {
            return disposables;
        }

        // Basic observables
        var pointerPressed = AssociatedObject
            .OnEvent(InputElement.PointerPressedEvent, RoutingStrategy)
            .Select(e => e.EventArgs.GetCurrentPoint(container))
            .Where(point => point.Properties.IsButtonPressed(DragButton));

        var pointerMoved = AssociatedObject
            .OnEvent(InputElement.PointerMovedEvent, RoutingStrategy)
            .Select(e => e.EventArgs.GetCurrentPoint(container).Position);

        var pointerReleased = AssociatedObject.OnEvent(InputElement.PointerReleasedEvent, RoutingStrategy);
        var captureLost = AssociatedObject.OnEvent(InputElement.PointerCaptureLostEvent);

        pointerPressed
            .Do(point =>
            {
                point.Pointer.Capture(AssociatedObject);
                lastPosition = point.Position;
            })
            .SelectMany(_ =>
                pointerMoved
                    .TakeUntil(
                        pointerReleased
                            .Do(__ =>
                            {
                                _.Pointer.Capture(null);
                                lastPosition = null;
                            })
                            .ToSignal()
                            .Merge(captureLost.ToSignal())
                    )
                    .Where(_ => lastPosition.HasValue)
                    .Select(currentPosition =>
                    {
                        var delta = currentPosition - lastPosition!.Value;
                        lastPosition = currentPosition;
                        return delta;
                    })
                    .Do(ApplyDelta)
            )
            .Repeat()
            .Subscribe()
            .DisposeWith(disposables);

        return disposables;
    }

    private void ApplyDelta(Point delta)
    {
        if (!IsEnabled)
        {
            return;
        }

        Left += delta.X;
        Top += delta.Y;
    }
}