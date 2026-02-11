using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Styling;

namespace Zafiro.Avalonia.Controls;

public class HeaderedContainer : ContentControl
{
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty = Border.BoxShadowProperty.AddOwner<HeaderedContainer>();

    public static readonly StyledProperty<IBrush?> HeaderBackgroundProperty = AvaloniaProperty.Register<HeaderedContainer, IBrush?>(
        nameof(HeaderBackground));

    public static readonly StyledProperty<IBrush?> ContentBackgroundProperty = AvaloniaProperty.Register<HeaderedContainer, IBrush?>(
        nameof(ContentBackground));

    public static readonly StyledProperty<object> HeaderProperty = AvaloniaProperty.Register<HeaderedContainer, object>(
        nameof(Header));

    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty = AvaloniaProperty.Register<HeaderedContainer, IDataTemplate>(
        nameof(HeaderTemplate));

    public static readonly StyledProperty<Thickness?> HeaderPaddingProperty = AvaloniaProperty.Register<HeaderedContainer, Thickness?>(
        nameof(HeaderPadding));

    public static readonly StyledProperty<ControlTheme> ContentThemeProperty = AvaloniaProperty.Register<HeaderedContainer, ControlTheme>(
        nameof(ContentTheme));

    public static readonly StyledProperty<Thickness?> ContentPaddingProperty = AvaloniaProperty.Register<HeaderedContainer, Thickness?>(
        nameof(ContentPadding));

    public static readonly StyledProperty<ControlTheme> HeaderThemeProperty = AvaloniaProperty.Register<HeaderedContainer, ControlTheme>(
        nameof(HeaderTheme));

    public static readonly StyledProperty<string> HeaderClassesProperty = AvaloniaProperty.Register<HeaderedContainer, string>(
        nameof(HeaderClasses));

    public static readonly StyledProperty<string> ContentClassesProperty = AvaloniaProperty.Register<HeaderedContainer, string>(
        nameof(ContentClasses));

    public static readonly DirectProperty<HeaderedContainer, Thickness> EffectiveHeaderPaddingProperty =
        AvaloniaProperty.RegisterDirect<HeaderedContainer, Thickness>(
            nameof(EffectiveHeaderPadding),
            o => o.EffectiveHeaderPadding);

    public static readonly DirectProperty<HeaderedContainer, Thickness> EffectiveContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<HeaderedContainer, Thickness>(
            nameof(EffectiveContentPadding),
            o => o.EffectiveContentPadding);

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

    public IBrush? ContentBackground
    {
        get => GetValue(ContentBackgroundProperty);
        set => SetValue(ContentBackgroundProperty, value);
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

    public Thickness? HeaderPadding
    {
        get => GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    public Thickness? ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    public string HeaderClasses
    {
        get => GetValue(HeaderClassesProperty);
        set => SetValue(HeaderClassesProperty, value);
    }

    public string ContentClasses
    {
        get => GetValue(ContentClassesProperty);
        set => SetValue(ContentClassesProperty, value);
    }

    /// <summary>
    /// Gets the effective padding for the header. Returns HeaderPadding if set, otherwise falls back to Padding.
    /// </summary>
    public Thickness EffectiveHeaderPadding => HeaderPadding ?? Padding;

    /// <summary>
    /// Gets the effective padding for the content. Returns ContentPadding if set, otherwise falls back to Padding.
    /// </summary>
    public Thickness EffectiveContentPadding => ContentPadding ?? Padding;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PaddingProperty || change.Property == HeaderPaddingProperty)
        {
            var oldValue = change.Property == HeaderPaddingProperty
                ? (Thickness?)change.OldValue ?? Padding
                : HeaderPadding ?? (Thickness)change.OldValue!;
            RaisePropertyChanged(EffectiveHeaderPaddingProperty, oldValue, EffectiveHeaderPadding);
        }

        if (change.Property == PaddingProperty || change.Property == ContentPaddingProperty)
        {
            var oldValue = change.Property == ContentPaddingProperty
                ? (Thickness?)change.OldValue ?? Padding
                : ContentPadding ?? (Thickness)change.OldValue!;
            RaisePropertyChanged(EffectiveContentPaddingProperty, oldValue, EffectiveContentPadding);
        }
    }
}