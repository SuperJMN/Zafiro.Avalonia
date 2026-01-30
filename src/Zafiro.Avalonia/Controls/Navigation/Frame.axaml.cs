using System.Windows.Input;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Zafiro.Avalonia.Controls.Navigation;

[TemplatePart("BackButton", typeof(EnhancedButton))]
[TemplatePart("Content", typeof(ContentPresenter))]
[TemplatePart("Header", typeof(ContentPresenter))]
[TemplatePart("Footer", typeof(ContentPresenter))]
public class Frame : TemplatedControl
{
    public new static readonly StyledProperty<IBrush?> BackgroundProperty = Border.BackgroundProperty.AddOwner<Frame>();
    public new static readonly StyledProperty<IBrush?> BorderBrushProperty = Border.BorderBrushProperty.AddOwner<Frame>();
    public new static readonly StyledProperty<Thickness> BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner<Frame>();
    public new static readonly StyledProperty<CornerRadius> CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner<Frame>();
    public new static readonly StyledProperty<Thickness> PaddingProperty = Decorator.PaddingProperty.AddOwner<Frame>();
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty = Border.BoxShadowProperty.AddOwner<Frame>();

    public static readonly StyledProperty<ICommand> BackCommandProperty = AvaloniaProperty.Register<Frame, ICommand>(
        nameof(BackCommand));

    public static readonly StyledProperty<object> ContentProperty = AvaloniaProperty.Register<Frame, object>(
        nameof(Content));

    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<Frame, object?>(
        nameof(Header));

    public static readonly StyledProperty<object?> FooterProperty = AvaloniaProperty.Register<Frame, object?>(
        nameof(Footer));

    public static readonly StyledProperty<IBrush?> HeaderBackgroundProperty = AvaloniaProperty.Register<Frame, IBrush?>(
        nameof(HeaderBackground));

    public static readonly StyledProperty<IBrush?> ContentBackgroundProperty = AvaloniaProperty.Register<Frame, IBrush?>(
        nameof(ContentBackground));

    public static readonly StyledProperty<IBrush?> FooterBackgroundProperty = AvaloniaProperty.Register<Frame, IBrush?>(
        nameof(FooterBackground));

    public static readonly StyledProperty<Thickness?> HeaderPaddingProperty = AvaloniaProperty.Register<Frame, Thickness?>(
        nameof(HeaderPadding));

    public static readonly StyledProperty<Thickness?> ContentPaddingProperty = AvaloniaProperty.Register<Frame, Thickness?>(
        nameof(ContentPadding));

    public static readonly StyledProperty<Thickness?> FooterPaddingProperty = AvaloniaProperty.Register<Frame, Thickness?>(
        nameof(FooterPadding));

    public static readonly DirectProperty<Frame, Thickness> EffectiveHeaderPaddingProperty = AvaloniaProperty.RegisterDirect<Frame, Thickness>(
        nameof(EffectiveHeaderPadding), o => o.EffectiveHeaderPadding);

    public static readonly DirectProperty<Frame, Thickness> EffectiveContentPaddingProperty = AvaloniaProperty.RegisterDirect<Frame, Thickness>(
        nameof(EffectiveContentPadding), o => o.EffectiveContentPadding);

    public static readonly DirectProperty<Frame, Thickness> EffectiveFooterPaddingProperty = AvaloniaProperty.RegisterDirect<Frame, Thickness>(
        nameof(EffectiveFooterPadding), o => o.EffectiveFooterPadding);

    public ICommand BackCommand
    {
        get => GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

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

    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
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

    public IBrush? FooterBackground
    {
        get => GetValue(FooterBackgroundProperty);
        set => SetValue(FooterBackgroundProperty, value);
    }

    public Thickness? HeaderPadding
    {
        get => GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    public Thickness? ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    public Thickness? FooterPadding
    {
        get => GetValue(FooterPaddingProperty);
        set => SetValue(FooterPaddingProperty, value);
    }

    public Thickness EffectiveHeaderPadding => HeaderPadding ?? Padding;
    public Thickness EffectiveContentPadding => ContentPadding ?? Padding;
    public Thickness EffectiveFooterPadding => FooterPadding ?? Padding;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PaddingProperty || change.Property == HeaderPaddingProperty)
        {
            RaisePropertyChanged(EffectiveHeaderPaddingProperty, default, EffectiveHeaderPadding);
        }

        if (change.Property == PaddingProperty || change.Property == ContentPaddingProperty)
        {
            RaisePropertyChanged(EffectiveContentPaddingProperty, default, EffectiveContentPadding);
        }

        if (change.Property == PaddingProperty || change.Property == FooterPaddingProperty)
        {
            RaisePropertyChanged(EffectiveFooterPaddingProperty, default, EffectiveFooterPadding);
        }
    }
}