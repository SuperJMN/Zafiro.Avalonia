using System.Reactive.Disposables;
using Avalonia.Controls.Primitives;
using Zafiro.Avalonia.Misc;
using Zafiro.Avalonia.Wizards.Graph.Core;
using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Wizards.Graph.View;

public class GraphWizardFooterView : TemplatedControl
{
    public static readonly StyledProperty<IGraphWizard> WizardProperty =
        AvaloniaProperty.Register<GraphWizardFooterView, IGraphWizard>(nameof(Wizard));

    public static readonly StyledProperty<object?> CurrentFooterProperty =
        AvaloniaProperty.Register<GraphWizardFooterView, object?>(nameof(CurrentFooter));

    private CompositeDisposable? subscriptions;

    public IGraphWizard Wizard
    {
        get => GetValue(WizardProperty);
        set => SetValue(WizardProperty, value);
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
                ? wizard.WhenAnyValue(x => x.CurrentStep)
                    .Select(step =>
                    {
                        if (step?.Content is IHaveFooter footer)
                        {
                            return footer.Footer;
                        }

                        return Observable.Return<object?>(null);
                    })
                    .Switch()
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