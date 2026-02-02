using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

public class GraphWizardFooterViewModel : ReactiveObject
{
    private readonly ObservableAsPropertyHelper<object?> currentFooter;

    public GraphWizardFooterViewModel(GraphWizard wizard)
    {
        Wizard = wizard;

        currentFooter = wizard
            .WhenAnyValue(x => x.CurrentStep)
            .Select(step =>
            {
                if (step?.Content is IHaveFooter haveFooter)
                {
                    return haveFooter.Footer;
                }

                return Observable.Return<object?>(null);
            })
            .Switch()
            .ToProperty(this, x => x.CurrentFooter);
    }

    public GraphWizard Wizard { get; }
    public object? CurrentFooter => currentFooter.Value;
}