using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using DynamicData.Binding;
using ReactiveUI;

namespace Zafiro.Avalonia.DataViz.Monitoring;

public class VerticalScale : Control
{
    public static readonly StyledProperty<IEnumerable<double>?> ValuesProperty = AvaloniaProperty.Register<VerticalScale, IEnumerable<double>?>(
        nameof(Values));

    public static readonly StyledProperty<double> XSpacingProperty = AvaloniaProperty.Register<VerticalScale, double>(
        nameof(XSpacing));

    public static readonly StyledProperty<double> LineIntervalProperty = AvaloniaProperty.Register<VerticalScale, double>(
        nameof(LineInterval), 10); // Default interval of 10 units

    public static readonly StyledProperty<double> StrokeThicknessProperty = AvaloniaProperty.Register<VerticalScale, double>(
        nameof(StrokeThickness), 1d);

    public static readonly StyledProperty<IBrush> StrokeProperty = AvaloniaProperty.Register<VerticalScale, IBrush>(
        nameof(Stroke));

    public static readonly StyledProperty<IBrush> ZeroStrokeProperty = AvaloniaProperty.Register<VerticalScale, IBrush>(
        nameof(ZeroStroke));

    private readonly IDisposable collectionChangesSubscription;

    public VerticalScale()
    {
        var collections = this
            .WhenAnyValue(x => x.Values)
            .Select(x => x as INotifyCollectionChanged)
            .WhereNotNull();

        var changes = collections
            .Select(a => a.ObserveCollectionChanges())
            .Switch();

        collectionChangesSubscription = changes
            .ObserveOn(AvaloniaScheduler.Instance)
            .Do(_ =>
            {
                InvalidateVisual();
                InvalidateMeasure();
            })
            .Subscribe();
    }

    public IEnumerable<double>? Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public double XSpacing
    {
        get => GetValue(XSpacingProperty);
        set => SetValue(XSpacingProperty, value);
    }

    public double LineInterval
    {
        get => GetValue(LineIntervalProperty);
        set => SetValue(LineIntervalProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public IBrush Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public IBrush ZeroStroke
    {
        get => GetValue(ZeroStrokeProperty);
        set => SetValue(ZeroStrokeProperty, value);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        collectionChangesSubscription.Dispose();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Values is null || !Values.Any())
        {
            return new Size();
        }

        return new Size(0, Values.Max());
    }


    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Values is null || !Values.Any())
        {
            return;
        }

        var valuesArray = Values.ToArray();

        double minValue = valuesArray.Min();
        double maxValue = valuesArray.Max();

        var height = Bounds.Height;
        var width = Bounds.Width;

        // Get the effective scaling factor
        var effectiveScale = GetEffectiveScale();
        var scaleX = effectiveScale.X;
        var scaleY = effectiveScale.Y;

        // Adjust the line thickness and font size
        double adjustedStrokeThickness = StrokeThickness / scaleY; // Keep a constant thickness of 1 unit

        // Draw the zero line
        var zeroY = TransformY(0, minValue, maxValue, height);
        var middlePen = new Pen(ZeroStroke, adjustedStrokeThickness);
        context.DrawLine(middlePen, new Point(0, zeroY), new Point(width, zeroY));

        // Configure the interval and style for the horizontal lines
        var interval = LineInterval;
        var linePen = new Pen(Stroke, adjustedStrokeThickness, dashStyle: DashStyle.Dash);

        // Calculate the range of values for the lines
        double startValue = Math.Floor(minValue / interval) * interval;
        double endValue = Math.Ceiling(maxValue / interval) * interval;

        // Draw the horizontal lines and labels
        for (double value = startValue; value <= endValue; value += interval)
        {
            var y = TransformY(value, minValue, maxValue, height);

            // Draw the horizontal line
            if (value != 0)
            {
                context.DrawLine(linePen, new Point(0, y), new Point(width, y));
            }
        }
    }

    private Vector GetEffectiveScale()
    {
        var transform = this.GetTransformedBounds();
        if (transform == null)
            return new Vector(1, 1);

        var matrix = transform.Value.Transform;
        var scaleX = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
        var scaleY = Math.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22);

        // Avoid division by zero
        scaleX = scaleX == 0 ? 1 : scaleX;
        scaleY = scaleY == 0 ? 1 : scaleY;

        return new Vector(scaleX, scaleY);
    }

    private double TransformY(double value, double minValue, double maxValue, double height)
    {
        // Invert the Y axis so larger values stay at the top
        double range = maxValue - minValue;
        if (range == 0)
        {
            return height / 2;
        }
        else
        {
            return height - ((value - minValue) / range) * height;
        }
    }
}