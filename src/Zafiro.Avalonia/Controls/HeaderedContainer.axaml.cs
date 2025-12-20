using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Styling;

namespace Zafiro.Avalonia.Controls;

public class HeaderedContainer : ContentControl
{
    public static readonly StyledProperty<IBrush> HeaderBackgroundProperty = AvaloniaProperty.Register<HeaderedContainer, IBrush>(
        nameof(HeaderBackground));

    public static readonly StyledProperty<object> HeaderProperty = AvaloniaProperty.Register<HeaderedContainer, object>(
        nameof(Header));

    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty = AvaloniaProperty.Register<HeaderedContainer, IDataTemplate>(
        nameof(HeaderTemplate));

    public static readonly StyledProperty<Thickness> HeaderPaddingProperty = AvaloniaProperty.Register<HeaderedContainer, Thickness>(
        nameof(HeaderPadding));

    public static readonly StyledProperty<BoxShadows> BoxShadowProperty = AvaloniaProperty.Register<HeaderedContainer, BoxShadows>(
        nameof(BoxShadow));

    public static readonly StyledProperty<ControlTheme> ContentThemeProperty = AvaloniaProperty.Register<HeaderedContainer, ControlTheme>(
        nameof(ContentTheme));

    public static readonly StyledProperty<Thickness> ContentPaddingProperty = AvaloniaProperty.Register<HeaderedContainer, Thickness>(
        nameof(ContentPadding));

    public static readonly StyledProperty<ControlTheme> HeaderThemeProperty = AvaloniaProperty.Register<HeaderedContainer, ControlTheme>(
        nameof(HeaderTheme));

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

    public IBrush HeaderBackground
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
}