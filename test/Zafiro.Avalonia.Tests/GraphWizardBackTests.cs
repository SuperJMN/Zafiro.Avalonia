using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using Zafiro.Avalonia.Wizards.Graph.Core;
using WizardGraph = Zafiro.Avalonia.Wizards.Graph.Core.GraphWizard;

namespace Zafiro.Avalonia.Tests;

public class GraphWizardBackTests
{
    [Fact]
    public async Task Back_command_combines_history_with_external_gate()
    {
        var canGoBack = new BehaviorSubject<bool>(false);
        var wizard = CreateTwoStepWizard(canGoBack: canGoBack);

        await wizard.Next.Execute();

        Assert.False(CanExecuteBack(wizard));

        canGoBack.OnNext(true);
        await wizard.Back.CanExecute.FirstAsync(x => x).ToTask();

        Assert.True(CanExecuteBack(wizard));
    }

    [Fact]
    public async Task Back_command_still_requires_history_when_external_gate_allows_back()
    {
        var canGoBack = new BehaviorSubject<bool>(true);
        var wizard = CreateTwoStepWizard(canGoBack: canGoBack);

        Assert.False(CanExecuteBack(wizard));

        await wizard.Next.Execute();
        await wizard.Back.CanExecute.FirstAsync(x => x).ToTask();

        Assert.True(CanExecuteBack(wizard));
    }

    private static GraphWizard<string> CreateTwoStepWizard(IObservable<bool>? canGoBack = null)
    {
        var graph = WizardGraph.For<string>();
        var second = graph.Step(() => new StepModel("second"), "Second")
            .Finish(model => model.Value)
            .Build();
        var first = graph.Step(() => new StepModel("first"), "First")
            .Next(_ => second)
            .Build();

        return new GraphWizard<string>(first, canGoBack: canGoBack);
    }

    private static bool CanExecuteBack(IGraphWizard wizard)
    {
        return ((ICommand)wizard.Back).CanExecute(null);
    }

    private sealed record StepModel(string Value);
}
