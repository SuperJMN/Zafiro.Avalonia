using System.Windows.Input;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Zafiro.Avalonia.Controls.Navigation;

[TemplatePart("BackButton", typeof(EnhancedButton))]
[TemplatePart("Content", typeof(ContentPresenter))]
[TemplatePart("Header", typeof(ContentPresenter))]
[TemplatePart("Footer", typeof(ContentPresenter))]
public class Frame : ContentControl
{
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty = Border.BoxShadowProperty.AddOwner<Frame>();

    public static readonly StyledProperty<ICommand> BackCommandProperty = AvaloniaProperty.Register<Frame, ICommand>(
        nameof(BackCommand));

    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<Frame, object?>(
        nameof(Header));

    public static readonly StyledProperty<object?> FooterProperty = AvaloniaProperty.Register<Frame, object?>(
        nameof(Footer));

    public static readonly StyledProperty<FrameHeaderDisplayMode> HeaderDisplayModeProperty = AvaloniaProperty.Register<Frame, FrameHeaderDisplayMode>(
        nameof(HeaderDisplayMode));

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

    public FrameHeaderDisplayMode HeaderDisplayMode
    {
        get => GetValue(HeaderDisplayModeProperty);
        set => SetValue(HeaderDisplayModeProperty, value);
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        topLevel?.AddHandler(TopLevel.BackRequestedEvent, OnSystemBackRequested);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        topLevel?.RemoveHandler(TopLevel.BackRequestedEvent, OnSystemBackRequested);
        base.OnDetachedFromVisualTree(e);
    }

    private void OnSystemBackRequested(object? sender, RoutedEventArgs e)
    {
        if (BackCommand?.CanExecute(null) == true)
        {
            BackCommand.Execute(null);
            e.Handled = true;
        }
    }
}
