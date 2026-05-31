using System.Globalization;
using Avalonia.Controls;
using Avalonia.Media;

namespace Zafiro.Avalonia.Controls;

public class OutlinedText : Control
{
    public static readonly StyledProperty<IBrush?> BackgroundProperty = TextBlock.BackgroundProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<Thickness> PaddingProperty = TextBlock.PaddingProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<string?> TextProperty = TextBlock.TextProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<FontFamily> FontFamilyProperty = TextBlock.FontFamilyProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<double> FontSizeProperty = TextBlock.FontSizeProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<FontStyle> FontStyleProperty = TextBlock.FontStyleProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<FontWeight> FontWeightProperty = TextBlock.FontWeightProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<FontStretch> FontStretchProperty = TextBlock.FontStretchProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<IBrush?> ForegroundProperty = TextBlock.ForegroundProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<double> LineHeightProperty = TextBlock.LineHeightProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<int> MaxLinesProperty = TextBlock.MaxLinesProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty = TextBlock.TextAlignmentProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<TextWrapping> TextWrappingProperty = TextBlock.TextWrappingProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<TextTrimming> TextTrimmingProperty = TextBlock.TextTrimmingProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<TextDecorationCollection?> TextDecorationsProperty = TextBlock.TextDecorationsProperty.AddOwner<OutlinedText>();
    public static readonly StyledProperty<FontFeatureCollection?> FontFeaturesProperty = TextBlock.FontFeaturesProperty.AddOwner<OutlinedText>();

    public static readonly StyledProperty<IBrush?> StrokeProperty = AvaloniaProperty.Register<OutlinedText, IBrush?>(
        nameof(Stroke), Brushes.Black);

    public static readonly StyledProperty<double> StrokeThicknessProperty = AvaloniaProperty.Register<OutlinedText, double>(
        nameof(StrokeThickness));

    static OutlinedText()
    {
        AffectsMeasure<OutlinedText>(
            PaddingProperty,
            TextProperty,
            FontFamilyProperty,
            FontSizeProperty,
            FontStyleProperty,
            FontWeightProperty,
            FontStretchProperty,
            LineHeightProperty,
            MaxLinesProperty,
            TextAlignmentProperty,
            TextWrappingProperty,
            TextTrimmingProperty,
            TextDecorationsProperty,
            FontFeaturesProperty,
            FlowDirectionProperty,
            StrokeThicknessProperty);

        AffectsRender<OutlinedText>(
            BackgroundProperty,
            PaddingProperty,
            TextProperty,
            FontFamilyProperty,
            FontSizeProperty,
            FontStyleProperty,
            FontWeightProperty,
            FontStretchProperty,
            ForegroundProperty,
            LineHeightProperty,
            MaxLinesProperty,
            TextAlignmentProperty,
            TextWrappingProperty,
            TextTrimmingProperty,
            TextDecorationsProperty,
            FontFeaturesProperty,
            FlowDirectionProperty,
            StrokeProperty,
            StrokeThicknessProperty);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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

    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
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

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    public int MaxLines
    {
        get => GetValue(MaxLinesProperty);
        set => SetValue(MaxLinesProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public TextTrimming TextTrimming
    {
        get => GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    public TextDecorationCollection? TextDecorations
    {
        get => GetValue(TextDecorationsProperty);
        set => SetValue(TextDecorationsProperty, value);
    }

    public FontFeatureCollection? FontFeatures
    {
        get => GetValue(FontFeaturesProperty);
        set => SetValue(FontFeaturesProperty, value);
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var layout = CreateLayout(GetAvailableTextWidth(availableSize), new Point());
        TextBlock.SetBaselineOffset(this, Padding.Top + layout.TopInset + layout.Text.Baseline);

        return new Size(
            layout.Width + Padding.Left + Padding.Right,
            layout.Height + Padding.Top + Padding.Bottom);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Background is not null)
        {
            context.DrawRectangle(Background, null, new Rect(Bounds.Size));
        }

        if (string.IsNullOrEmpty(Text))
        {
            return;
        }

        var preliminary = CreateLayout(GetAvailableTextWidth(Bounds.Size), new Point());
        var origin = new Point(Padding.Left + preliminary.LeftInset, Padding.Top + preliminary.TopInset);
        var layout = CreateLayout(GetAvailableTextWidth(Bounds.Size), origin);
        var pen = EffectiveStrokeThickness > 0 && Stroke is not null ? new Pen(Stroke, EffectiveStrokeThickness) : null;

        context.DrawGeometry(Foreground, pen, layout.Geometry);
    }

    private double GetAvailableTextWidth(Size availableSize)
    {
        if (TextWrapping == TextWrapping.NoWrap || double.IsInfinity(availableSize.Width))
        {
            return 0;
        }

        return Math.Max(0, availableSize.Width - Padding.Left - Padding.Right - EffectiveStrokeThickness);
    }

    private OutlinedTextLayout CreateLayout(double availableTextWidth, Point origin)
    {
        var text = Text ?? string.Empty;
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection, typeface, FontSize, Foreground)
        {
            LineHeight = LineHeight,
            TextAlignment = TextAlignment,
            Trimming = TextTrimming,
        };

        if (MaxLines > 0)
        {
            formattedText.MaxLineCount = MaxLines;
        }

        if (TextWrapping != TextWrapping.NoWrap)
        {
            formattedText.MaxTextWidth = availableTextWidth;
        }

        if (TextDecorations is not null)
        {
            formattedText.SetTextDecorations(TextDecorations);
        }

        if (FontFeatures is not null)
        {
            formattedText.SetFontFeatures(FontFeatures);
        }

        var geometryAtZero = formattedText.BuildGeometry(new Point()) ?? new StreamGeometry();
        var geometryBounds = geometryAtZero.Bounds;
        var halfStroke = EffectiveStrokeThickness / 2;
        var leftInset = Math.Max(0, -geometryBounds.Left) + halfStroke;
        var topInset = Math.Max(0, -geometryBounds.Top) + halfStroke;
        var rightInset = Math.Max(0, geometryBounds.Right - formattedText.Width) + halfStroke;
        var bottomInset = Math.Max(0, geometryBounds.Bottom - formattedText.Height) + halfStroke;
        var geometry = formattedText.BuildGeometry(origin) ?? new StreamGeometry();

        return new OutlinedTextLayout(
            formattedText,
            geometry,
            formattedText.Width + leftInset + rightInset,
            formattedText.Height + topInset + bottomInset,
            leftInset,
            topInset);
    }

    private double EffectiveStrokeThickness => Math.Max(0, StrokeThickness);

    private sealed record OutlinedTextLayout(
        FormattedText Text,
        Geometry Geometry,
        double Width,
        double Height,
        double LeftInset,
        double TopInset);
}
