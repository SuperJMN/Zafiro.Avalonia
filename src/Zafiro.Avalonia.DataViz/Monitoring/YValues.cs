using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using DynamicData.Binding;
using ReactiveUI;

namespace Zafiro.Avalonia.DataViz.Monitoring;

public class YValues : Control
{
    public static readonly StyledProperty<IEnumerable<double>> ValuesProperty = AvaloniaProperty.Register<YValues, IEnumerable<double>>(
        nameof(Values));

    public static readonly StyledProperty<double> LineIntervalProperty = AvaloniaProperty.Register<YValues, double>(
        nameof(LineInterval));

    /// <summary>
    /// Defines the <see cref="Foreground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty = TextElement.ForegroundProperty.AddOwner<VerticalScale>();

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<SamplerControl>();

    /// <summary>
    /// Defines the <see cref="FontFeaturesProperty"/> property.
    /// </summary>
    public static readonly StyledProperty<FontFeatureCollection?> FontFeaturesProperty =
        TextElement.FontFeaturesProperty.AddOwner<SamplerControl>();

    /// <summary>
    /// Defines the <see cref="ExCSS.FontSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<SamplerControl>();

    /// <summary>
    /// Defines the <see cref="FontStyle"/> property.
    /// </summary>
    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        TextElement.FontStyleProperty.AddOwner<SamplerControl>();

    /// <summary>
    /// Defines the <see cref="FontWeight"/> property.
    /// </summary>
    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        TextElement.FontWeightProperty.AddOwner<SamplerControl>();

    /// <summary>
    /// Defines the <see cref="FontWeight"/> property.
    /// </summary>
    public static readonly StyledProperty<FontStretch> FontStretchProperty =
        TextElement.FontStretchProperty.AddOwner<SamplerControl>();

    private readonly IDisposable collectionChangesSubscription;

    public YValues()
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

    public IEnumerable<double> Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public double LineInterval
    {
        get => GetValue(LineIntervalProperty);
        set => SetValue(LineIntervalProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public FontStretch FontStretch
    {
        get => GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var values = Values?.ToList();

        if (values is null || !values.Any())
        {
            return new Size();
        }

        double minValue = values.Min();
        double maxValue = values.Max();
        double interval = LineInterval;

        double startValue = Math.Floor(minValue / interval) * interval;
        double endValue = Math.Ceiling(maxValue / interval) * interval;

        var width = new[] { startValue, endValue }.Max(d => FormatText(d.ToString(CultureInfo.InvariantCulture)).Width);
        var height = (values.Max() - values.Min()) * LineInterval;

        return new Size(width, height);
    }

    private FormattedText FormatText(string str)
    {
        // Configure the font for the labels
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var textBrush = Foreground;

        // Obtain the effective scaling factor
        var effectiveScale = GetEffectiveScale();
        var scaleX = effectiveScale.X;
        var scaleY = effectiveScale.Y;

        // Adjust the line thickness and the font size
        double adjustedStrokeThickness = 1 / scaleY; // Keep a constant thickness of 1 unit
        double adjustedFontSize = 10 / scaleY; // Keep a constant font size of 10 units

        var formattedText = new FormattedText(
            textToFormat: str,
            culture: CultureInfo.CurrentCulture,
            flowDirection: FlowDirection.LeftToRight,
            typeface: typeface,
            emSize: adjustedFontSize,
            foreground: textBrush);

        return formattedText;
    }

    private static double TransformY(double value, double minValue, double maxValue, double height)
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

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        collectionChangesSubscription.Dispose();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var values = Values?.ToArray();

        if (values is null || !values.Any())
        {
            return;
        }

        double minValue = values.Min();
        double maxValue = values.Max();

        var height = Bounds.Height;
        var width = Bounds.Width;

        // Obtain the effective scaling factor
        var effectiveScale = GetEffectiveScale();
        var scaleX = effectiveScale.X;
        var scaleY = effectiveScale.Y;

        // Adjust the line thickness and the font size

        // Configure the interval and style of the horizontal lines
        var interval = LineInterval;

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
                //context.DrawLine(linePen, new Point(0, y), new Point(width, y));
            }

            // Create the formatted text
            var formattedText = FormatText(value.ToString(CultureInfo.InvariantCulture));

            // Position the label on the left of the line
            var textPosition = new Point(0, y - formattedText.Height / 2);
            context.DrawText(formattedText, textPosition);
        }
    }

    private Vector GetEffectiveScale()
    {
        var visualRoot = VisualRoot as Visual;
        if (visualRoot == null)
            return new Vector(1, 1);

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
}