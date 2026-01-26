using System.Reactive.Disposables;
using Avalonia.Controls.Primitives;
using Zafiro.Avalonia.Misc;
using Zafiro.UI.Commands;
using Zafiro.UI.Navigation;
using Zafiro.UI.Wizards.Slim;

namespace Zafiro.Avalonia.Controls.Wizards.Slim;

public class WizardFooter : TemplatedControl
{
    public static readonly StyledProperty<ISlimWizard> WizardProperty = AvaloniaProperty.Register<WizardFooter, ISlimWizard>(
        nameof(Wizard));

    public static readonly StyledProperty<object?> CurrentFooterProperty = AvaloniaProperty.Register<WizardFooter, object?>(
        nameof(CurrentFooter));

    public static readonly StyledProperty<IEnhancedCommand?> CancelProperty = AvaloniaProperty.Register<WizardFooter, IEnhancedCommand?>(
        nameof(Cancel));

    private CompositeDisposable? subscriptions;

    public ISlimWizard Wizard
    {
        get => GetValue(WizardProperty);
        set => SetValue(WizardProperty, value);
    }

    public object? CurrentFooter
    {
        get => GetValue(CurrentFooterProperty);
        set => SetValue(CurrentFooterProperty, value);
    }

    public IEnhancedCommand? Cancel
    {
        get => GetValue(CancelProperty);
        set => SetValue(CancelProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        subscriptions = new CompositeDisposable();

        this.GetObservable(WizardProperty)
            .Select(wizard => wizard is not null
                ? wizard.WhenAnyValue(x => x.CurrentPage)
                    .Select(page => GetFooterObservable(page?.Content))
                    .Switch()
                : Observable.Return<object?>(null))
            .Switch()
            .BindTo(this, CurrentFooterProperty)
            .DisposeWith(subscriptions);
    }

    private static IObservable<object?> GetFooterObservable(object? content)
    {
        if (content is IHaveFooter h)
        {
            return h.Footer;
        }

        return Observable.Return<object?>(null);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        subscriptions?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }
}