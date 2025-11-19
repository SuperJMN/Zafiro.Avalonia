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


    public static readonly StyledProperty<double> DragThresholdProperty = AvaloniaProperty.Register<DragDeltaBehavior, double>(nameof(DragThreshold), 3.0);

    private Point? lastPosition;

    public double DragThreshold
    {
        get => GetValue(DragThresholdProperty);
        set => SetValue(DragThresholdProperty, value);
    }

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

        // Usamos el primer ancestro que sea un Visual como sistema de coordenadas estable
        var container = AssociatedObject.FindAncestorOfType<Visual>();
        if (container is null)
        {
            return disposables;
        }

        // Observables básicos
        var pointerPressed = AssociatedObject
            .OnEvent(InputElement.PointerPressedEvent, RoutingStrategy)
            .Select(e => new { Point = e.EventArgs.GetCurrentPoint(container), e.EventArgs })
            .Where(x => x.Point.Properties.IsButtonPressed(DragButton));

        var pointerMoved = AssociatedObject
            .OnEvent(InputElement.PointerMovedEvent, RoutingStrategy)
            .Select(e => e.EventArgs.GetCurrentPoint(container).Position);

        var pointerReleased = AssociatedObject.OnEvent(InputElement.PointerReleasedEvent, RoutingStrategy);
        var captureLost = AssociatedObject.OnEvent(InputElement.PointerCaptureLostEvent);

        pointerPressed
            .SelectMany(start =>
            {
                var startPosition = start.Point.Position;
                var isDragging = false;

                return pointerMoved
                    .TakeUntil(
                        pointerReleased
                            .Do(released =>
                            {
                                if (isDragging)
                                {
                                    released.EventArgs.Pointer.Capture(null);
                                    isDragging = false;
                                    lastPosition = null;
                                }
                            })
                            .ToSignal()
                            .Merge(captureLost.ToSignal())
                    )
                    .Select(currentPosition =>
                    {
                        if (!isDragging)
                        {
                            var vector = currentPosition - startPosition;
                            if (Math.Abs(vector.X) > DragThreshold || Math.Abs(vector.Y) > DragThreshold)
                            {
                                isDragging = true;
                                start.EventArgs.Pointer.Capture(AssociatedObject);
                                start.EventArgs.Handled = true; // Prevent selection or other actions
                                lastPosition = currentPosition;
                                return (Point?)new Point(0, 0); // Initial delta is zero or accum? Let's say zero for smooth start
                            }

                            return null;
                        }
                        else
                        {
                            var delta = currentPosition - lastPosition!.Value;
                            lastPosition = currentPosition;
                            return delta;
                        }
                    })
                    .Where(delta => delta.HasValue)
                    .Select(delta => delta!.Value)
                    .Do(ApplyDelta);
            })
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