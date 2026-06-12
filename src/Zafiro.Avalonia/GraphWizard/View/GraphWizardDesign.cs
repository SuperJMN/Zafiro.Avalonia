using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Wizards.Graph.View;

public class GraphWizardDesign : GraphWizard
{
    public GraphWizardDesign() : base(new WizardNodeDesign())
    {
    }
}

public class WizardNodeDesign : IBaseWizardNode
{
    public IObservable<string> Title => Observable.Return("Sample Step Title");
    public object Content => "Sample step content";
    public IObservable<string> NextLabel => Observable.Return("Next");
}