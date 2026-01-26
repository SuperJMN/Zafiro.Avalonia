using System.Reactive.Disposables;
using System.Windows.Input;
using Avalonia.Controls.Primitives;
using Zafiro.Avalonia.Misc;
using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Controls.Navigation;

public class Frame : TemplatedControl
{
    public static readonly StyledProperty<ICommand> BackCommandProperty =
        AvaloniaProperty.Register<Frame, ICommand>(nameof(BackCommand));

    public static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<Frame, object>(nameof(Content));

    public static readonly StyledProperty<object> BackButtonContentProperty =
        AvaloniaProperty.Register<Frame, object>(nameof(BackButtonContent));

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<Frame, object?>(nameof(Header));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<Frame, object?>(nameof(Footer));

    CompositeDisposable? subscriptions;

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

    public object BackButtonContent
    {
        get => GetValue(BackButtonContentProperty);
        set => SetValue(BackButtonContentProperty, value);
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        subscriptions?.Dispose();
        subscriptions = new CompositeDisposable();

        this.GetObservable(ContentProperty)
            .Select(content => content is IHaveHeader haveHeader ? haveHeader.Header : null)
            .DistinctUntilChanged()
            .BindTo(this, HeaderProperty)
            .DisposeWith(subscriptions);

        this.GetObservable(ContentProperty)
            .Select(content => content is IHaveFooter haveFooter ? haveFooter.Footer : null)
            .DistinctUntilChanged()
            .BindTo(this, FooterProperty)
            .DisposeWith(subscriptions);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        subscriptions?.Dispose();
        subscriptions = null;

        base.OnDetachedFromVisualTree(e);
    }
}