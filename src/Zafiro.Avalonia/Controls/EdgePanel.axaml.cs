using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace Zafiro.Avalonia.Controls;

public class EdgePanel : ContentControl
{
    public new static readonly StyledProperty<IBrush?> BackgroundProperty = Border.BackgroundProperty.AddOwner<EdgePanel>();
    public new static readonly StyledProperty<IBrush?> BorderBrushProperty = Border.BorderBrushProperty.AddOwner<EdgePanel>();
    public new static readonly StyledProperty<Thickness> BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner<EdgePanel>();
    public new static readonly StyledProperty<CornerRadius> CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner<EdgePanel>();
    public new static readonly StyledProperty<Thickness> PaddingProperty = Decorator.PaddingProperty.AddOwner<EdgePanel>();

    public static readonly StyledProperty<object> StartContentProperty = AvaloniaProperty.Register<EdgePanel, object>(
        nameof(StartContent));

    public static readonly StyledProperty<object> EndContentProperty = AvaloniaProperty.Register<EdgePanel, object>(
        nameof(EndContent));

    public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<EdgePanel, double>(
        nameof(Spacing));

    public static readonly StyledProperty<IDataTemplate> StartContentTemplateProperty = AvaloniaProperty.Register<EdgePanel, IDataTemplate>(
        nameof(StartContentTemplate));

    public static readonly StyledProperty<IDataTemplate> EndContentTemplateProperty = AvaloniaProperty.Register<EdgePanel, IDataTemplate>(
        nameof(EndContentTemplate));

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

    public new Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public object StartContent
    {
        get => GetValue(StartContentProperty);
        set => SetValue(StartContentProperty, value);
    }

    public object EndContent
    {
        get => GetValue(EndContentProperty);
        set => SetValue(EndContentProperty, value);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public IDataTemplate StartContentTemplate
    {
        get => GetValue(StartContentTemplateProperty);
        set => SetValue(StartContentTemplateProperty, value);
    }

    public IDataTemplate EndContentTemplate
    {
        get => GetValue(EndContentTemplateProperty);
        set => SetValue(EndContentTemplateProperty, value);
    }
}