using System.Reactive.Disposables;
using Avalonia.Controls.Primitives;
using Zafiro.Avalonia.Misc;
using Zafiro.Avalonia.Wizards.Graph.Core;
using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Wizards.Graph.View;

public class GraphWizardHeaderView : TemplatedControl
{
    public static readonly StyledProperty<IGraphWizard> WizardProperty =
        AvaloniaProperty.Register<GraphWizardHeaderView, IGraphWizard>(nameof(Wizard));

    public static readonly StyledProperty<object?> CurrentHeaderProperty =
        AvaloniaProperty.Register<GraphWizardHeaderView, object?>(nameof(CurrentHeader));

    private CompositeDisposable? subscriptions;

    public IGraphWizard Wizard
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
                ? wizard.WhenAnyValue(x => x.CurrentStep)
                    .Select(step =>
                    {
                        if (step is null)
                        {
                            return Observable.Return<object?>("Wizard");
                        }

                        var headerObs = GetHeaderObservable(step.Content);
                        var titleObs = step.Title.Select(t => (object)t).StartWith((object?)"Wizard");

                        return headerObs.CombineLatest(titleObs, (header, title) => header ?? title);
                    })
                    .Switch()
                : Observable.Return<object?>("Wizard"))
            .Switch()
            .BindTo(this, CurrentHeaderProperty)
            .DisposeWith(subscriptions);
    }

    private static IObservable<object?> GetHeaderObservable(object content)
    {
        if (content is IHaveHeader h)
        {
            return h.Header;
        }

        return Observable.Return<object?>(null);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        subscriptions?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }
}