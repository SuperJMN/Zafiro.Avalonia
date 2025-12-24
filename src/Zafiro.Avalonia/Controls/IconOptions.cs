using Avalonia.Layout;
using Avalonia.Media;

namespace Zafiro.Avalonia.Controls;

public class IconOptions
{
    public static readonly AttachedProperty<double> SizeProperty = AvaloniaProperty.RegisterAttached<IconOptions, AvaloniaObject, double>("Size", inherits: true, defaultValue: 32);
    public static readonly AttachedProperty<IBrush?> FillProperty = AvaloniaProperty.RegisterAttached<IconOptions, AvaloniaObject, IBrush?>("Fill", inherits: true);
    public static readonly AttachedProperty<IBrush?> StrokeProperty = AvaloniaProperty.RegisterAttached<IconOptions, AvaloniaObject, IBrush?>("Stroke", inherits: true);
    public static readonly AttachedProperty<Thickness> PaddingProperty = AvaloniaProperty.RegisterAttached<IconOptions, AvaloniaObject, Thickness>("Padding", inherits: true);
    public static readonly AttachedProperty<CornerRadius> CornerRadiusProperty = AvaloniaProperty.RegisterAttached<IconOptions, AvaloniaObject, CornerRadius>("CornerRadius", inherits: true);
    public static readonly AttachedProperty<IBrush?> BackgroundProperty = AvaloniaProperty.RegisterAttached<IconOptions, AvaloniaObject, IBrush?>("Background", inherits: true);
    public static readonly AttachedProperty<IBrush?> BorderBrushProperty = AvaloniaProperty.RegisterAttached<IconOptions, AvaloniaObject, IBrush?>("BorderBrush", inherits: true);
    public static readonly AttachedProperty<Thickness> BorderThicknessProperty = AvaloniaProperty.RegisterAttached<IconOptions, AvaloniaObject, Thickness>("BorderThickness", inherits: true);
    public static readonly AttachedProperty<Thickness> MarginProperty = AvaloniaProperty.RegisterAttached<IconOptions, AvaloniaObject, Thickness>("Margin", inherits: true);
    public static readonly AttachedProperty<HorizontalAlignment> HorizontalAlignmentProperty = AvaloniaProperty.RegisterAttached<IconOptions, AvaloniaObject, HorizontalAlignment>("HorizontalAlignment", inherits: true, defaultValue: HorizontalAlignment.Center);
    public static readonly AttachedProperty<VerticalAlignment> VerticalAlignmentProperty = AvaloniaProperty.RegisterAttached<IconOptions, AvaloniaObject, VerticalAlignment>("VerticalAlignment", inherits: true, defaultValue: VerticalAlignment.Center);

    public static void SetSize(AvaloniaObject obj, double value) => obj.SetValue(SizeProperty, value);
    public static double GetSize(AvaloniaObject obj) => obj.GetValue(SizeProperty);

    public static void SetFill(AvaloniaObject obj, IBrush? value) => obj.SetValue(FillProperty, value);
    public static IBrush? GetFill(AvaloniaObject obj) => obj.GetValue(FillProperty);

    public static void SetStroke(AvaloniaObject obj, IBrush? value) => obj.SetValue(StrokeProperty, value);
    public static IBrush? GetStroke(AvaloniaObject obj) => obj.GetValue(StrokeProperty);

    public static void SetPadding(AvaloniaObject obj, Thickness value) => obj.SetValue(PaddingProperty, value);
    public static Thickness GetPadding(AvaloniaObject obj) => obj.GetValue(PaddingProperty);

    public static void SetCornerRadius(AvaloniaObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);
    public static CornerRadius GetCornerRadius(AvaloniaObject obj) => obj.GetValue(CornerRadiusProperty);

    public static void SetBackground(AvaloniaObject obj, IBrush? value) => obj.SetValue(BackgroundProperty, value);
    public static IBrush? GetBackground(AvaloniaObject obj) => obj.GetValue(BackgroundProperty);

    public static void SetBorderBrush(AvaloniaObject obj, IBrush? value) => obj.SetValue(BorderBrushProperty, value);
    public static IBrush? GetBorderBrush(AvaloniaObject obj) => obj.GetValue(BorderBrushProperty);

    public static void SetBorderThickness(AvaloniaObject obj, Thickness value) => obj.SetValue(BorderThicknessProperty, value);
    public static Thickness GetBorderThickness(AvaloniaObject obj) => obj.GetValue(BorderThicknessProperty);

    public static void SetMargin(AvaloniaObject obj, Thickness value) => obj.SetValue(MarginProperty, value);
    public static Thickness GetMargin(AvaloniaObject obj) => obj.GetValue(MarginProperty);

    public static void SetHorizontalAlignment(AvaloniaObject obj, HorizontalAlignment value) => obj.SetValue(HorizontalAlignmentProperty, value);
    public static HorizontalAlignment GetHorizontalAlignment(AvaloniaObject obj) => obj.GetValue(HorizontalAlignmentProperty);

    public static void SetVerticalAlignment(AvaloniaObject obj, VerticalAlignment value) => obj.SetValue(VerticalAlignmentProperty, value);
    public static VerticalAlignment GetVerticalAlignment(AvaloniaObject obj) => obj.GetValue(VerticalAlignmentProperty);
}