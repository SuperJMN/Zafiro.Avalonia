using System.Reactive.Disposables;
using Avalonia.Controls.Primitives;
using Zafiro.Avalonia.Misc;
using Zafiro.UI.Navigation;
using Zafiro.UI.Wizards.Slim;

namespace Zafiro.Avalonia.Controls.Wizards.Slim;

public class WizardFooter : TemplatedControl
{
    public static readonly StyledProperty<ISlimWizard> WizardProperty = AvaloniaProperty.Register<WizardFooter, ISlimWizard>(
        nameof(Wizard));

    public static readonly StyledProperty<SlimWizardNavigationHost> HostProperty = AvaloniaProperty.Register<WizardFooter, SlimWizardNavigationHost>(
        nameof(Host));

    public static readonly StyledProperty<object?> CurrentFooterProperty = AvaloniaProperty.Register<WizardFooter, object?>(
        nameof(CurrentFooter));

    private CompositeDisposable? subscriptions;

    public ISlimWizard Wizard
    {
        get => GetValue(WizardProperty);
        set => SetValue(WizardProperty, value);
    }

    public SlimWizardNavigationHost Host
    {
        get => GetValue(HostProperty);
        set => SetValue(HostProperty, value);
    }

    public object? CurrentFooter
    {
        get => GetValue(CurrentFooterProperty);
        set => SetValue(CurrentFooterProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        subscriptions = new CompositeDisposable();

        this.GetObservable(WizardProperty)
            .Select(wizard => wizard is not null
                ? wizard.WhenAnyValue(x => x.CurrentPage)
                    .Select(page => page.Content)
                    .Select(content => content is IHaveFooter h ? h.Footer : null)
                : Observable.Return<object?>(null))
            .Switch()
            .BindTo(this, CurrentFooterProperty)
            .DisposeWith(subscriptions);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        subscriptions?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }
}