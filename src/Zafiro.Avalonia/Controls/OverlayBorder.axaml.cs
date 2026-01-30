using Avalonia.Media;

namespace Zafiro.Avalonia.Controls;

public class OverlayBorder : ContentControl
{
    public new static readonly StyledProperty<IBrush?> BackgroundProperty = Border.BackgroundProperty.AddOwner<OverlayBorder>();

    public new static readonly StyledProperty<IBrush?> BorderBrushProperty = Border.BorderBrushProperty.AddOwner<OverlayBorder>();

    public new static readonly StyledProperty<Thickness> BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner<OverlayBorder>();

    public new static readonly StyledProperty<CornerRadius> CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner<OverlayBorder>();

    public static readonly StyledProperty<BoxShadows> BoxShadowProperty = Border.BoxShadowProperty.AddOwner<OverlayBorder>();

    public new IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public new IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public new Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public new CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
}