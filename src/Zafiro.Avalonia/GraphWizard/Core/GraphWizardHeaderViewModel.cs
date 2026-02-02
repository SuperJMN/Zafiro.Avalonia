using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

public class GraphWizardHeaderViewModel : ReactiveObject
{
    private readonly ObservableAsPropertyHelper<object?> currentHeader;

    public GraphWizardHeaderViewModel(GraphWizard wizard)
    {
        Wizard = wizard;

        currentHeader = wizard
            .WhenAnyValue(x => x.CurrentStep)
            .Select(step =>
            {
                if (step?.Content is IHaveHeader haveHeader)
                {
                    return haveHeader.Header.Select(x => (object?)x);
                }

                // Fallback to Title if no custom header
                return (step?.Title ?? Observable.Return(""))
                    .Select(t => (object?)(string.IsNullOrEmpty(t) ? "Wizard" : t));
            })
            .Switch()
            .ToProperty(this, x => x.CurrentHeader);
    }

    public GraphWizard Wizard { get; }
    public object? CurrentHeader => currentHeader.Value;
}