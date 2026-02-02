using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Wizards.Graph.Core;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Wizards.Graph.View;

public class GraphWizardDesign : GraphWizard
{
    public GraphWizardDesign() : base(new WizardNodeDesign())
    {
    }
}

public class WizardNodeDesign : IWizardNode
{
    public IObservable<string> Title => Observable.Return("Sample Step Title");
    public object Content => "Sample step content";
    public IObservable<string> NextLabel => Observable.Return("Next");

    public IEnhancedCommand<Result<IWizardNode?>> Next { get; } =
        ReactiveCommand.CreateFromTask(() => Task.FromResult(Result.Success<IWizardNode?>(null)))
            .Enhance("Next");
}