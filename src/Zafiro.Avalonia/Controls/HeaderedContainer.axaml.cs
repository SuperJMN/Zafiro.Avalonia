using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Styling;

namespace Zafiro.Avalonia.Controls;

public class HeaderedContainer : ContentControl
{
    public new static readonly StyledProperty<IBrush?> BackgroundProperty = Border.BackgroundProperty.AddOwner<HeaderedContainer>();
    public new static readonly StyledProperty<IBrush?> BorderBrushProperty = Border.BorderBrushProperty.AddOwner<HeaderedContainer>();
    public new static readonly StyledProperty<Thickness> BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner<HeaderedContainer>();
    public new static readonly StyledProperty<CornerRadius> CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner<HeaderedContainer>();
    public new static readonly StyledProperty<Thickness> PaddingProperty = Decorator.PaddingProperty.AddOwner<HeaderedContainer>();
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty = Border.BoxShadowProperty.AddOwner<HeaderedContainer>();

    public static readonly StyledProperty<IBrush?> HeaderBackgroundProperty = AvaloniaProperty.Register<HeaderedContainer, IBrush?>(
        nameof(HeaderBackground));

    public static readonly StyledProperty<object> HeaderProperty = AvaloniaProperty.Register<HeaderedContainer, object>(
        nameof(Header));

    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty = AvaloniaProperty.Register<HeaderedContainer, IDataTemplate>(
        nameof(HeaderTemplate));

    public static readonly StyledProperty<Thickness> HeaderPaddingProperty = AvaloniaProperty.Register<HeaderedContainer, Thickness>(
        nameof(HeaderPadding));

    public static readonly StyledProperty<ControlTheme> ContentThemeProperty = AvaloniaProperty.Register<HeaderedContainer, ControlTheme>(
        nameof(ContentTheme));

    public static readonly StyledProperty<Thickness> ContentPaddingProperty = AvaloniaProperty.Register<HeaderedContainer, Thickness>(
        nameof(ContentPadding));

    public static readonly StyledProperty<ControlTheme> HeaderThemeProperty = AvaloniaProperty.Register<HeaderedContainer, ControlTheme>(
        nameof(HeaderTheme));

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


    public ControlTheme HeaderTheme
    {
        get => GetValue(HeaderThemeProperty);
        set => SetValue(HeaderThemeProperty, value);
    }

    public ControlTheme ContentTheme
    {
        get => GetValue(ContentThemeProperty);
        set => SetValue(ContentThemeProperty, value);
    }

    public IBrush? HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IDataTemplate HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public Thickness HeaderPadding
    {
        get => GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PaddingProperty)
        {
            SetValue(ContentPaddingProperty, change.NewValue);
        }
    }
}