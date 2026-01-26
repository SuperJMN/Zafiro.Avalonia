using System.Reactive.Disposables;
using Avalonia.Controls.Primitives;
using Zafiro.Avalonia.Misc;
using Zafiro.UI.Navigation;
using Zafiro.UI.Wizards.Slim;

namespace Zafiro.Avalonia.Controls.Wizards.Slim;

public class WizardHeader : TemplatedControl
{
    public static readonly StyledProperty<bool> IsBackButtonVisibleProperty =
        AvaloniaProperty.Register<WizardHeader, bool>(nameof(IsBackButtonVisible), defaultValue: true);

    public static readonly StyledProperty<ISlimWizard> WizardProperty = AvaloniaProperty.Register<WizardHeader, ISlimWizard>(
        nameof(Wizard));

    public static readonly StyledProperty<object?> CurrentHeaderProperty = AvaloniaProperty.Register<WizardHeader, object?>(
        nameof(CurrentHeader));

    private CompositeDisposable? subscriptions;

    public bool IsBackButtonVisible
    {
        get => GetValue(IsBackButtonVisibleProperty);
        set => SetValue(IsBackButtonVisibleProperty, value);
    }

    public ISlimWizard Wizard
    {
        get => GetValue(WizardProperty);
        set => SetValue(WizardProperty, value);
    }

    public object? CurrentHeader
    {
        get => GetValue(CurrentHeaderProperty);
        set => SetValue(CurrentHeaderProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        subscriptions = new CompositeDisposable();

        this.GetObservable(WizardProperty)
            .Select(wizard => wizard is not null
                ? wizard.WhenAnyValue(x => x.CurrentPage)
                    .Select(page =>
                    {
                        if (page?.Content is IHaveHeader h)
                        {
                            return Observable.Return(h.Header);
                        }

                        if (page is not null)
                        {
                            return page.TitleObservable.Select(x => (object)x);
                        }

                        return Observable.Return<object?>(null);
                    })
                    .Switch()
                : Observable.Return<object?>(null))
            .Switch()
            .BindTo(this, CurrentHeaderProperty)
            .DisposeWith(subscriptions);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        subscriptions?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }
}