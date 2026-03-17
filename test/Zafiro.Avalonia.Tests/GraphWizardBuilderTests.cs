using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using ReactiveUI;
using Zafiro.Avalonia.Wizards.Graph.Builder;
using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Tests;

public class GraphWizardBuilderTests
{
    [Fact]
    public async Task Typed_builder_context_allows_defining_nodes_without_repeating_result_type()
    {
        var graph = GraphWizardBuilder.For<string>();

        var finalNode = graph.Define(new object(), "Finish")
            .Finish(_ => "done")
            .Build();

        var startNode = graph.Define(new object(), "Start")
            .Next(_ => finalNode)
            .Build();

        var wizard = new GraphWizard<string>(startNode);
        var completion = wizard.Finished.ToTask();

        await wizard.Next.Execute();
        await wizard.Next.Execute();

        var result = await completion;

        Assert.Equal("done", result);
    }

    [Fact]
    public async Task Typed_builder_context_can_start_a_typed_flow()
    {
        var graph = GraphWizardBuilder.For<string>();
        var node = graph.StartWith(new StepModel("start"), "Start")
            .Step(new StepModel("finish"), "Finish")
            .Finish(model => model.Value);

        var wizard = new GraphWizard<string>(node);
        var completion = wizard.Finished.ToTask();

        await wizard.Next.Execute();
        await wizard.Next.Execute();

        var result = await completion;

        Assert.Equal("finish", result);
    }

    [Fact]
    public async Task Typed_builder_supports_branching_to_path_a_and_finishes()
    {
        var start = new BranchModel();
        var graph = GraphWizardBuilder.For<string>();

        var endNode = graph.Define(new object(), "End")
            .Finish(_ => "done")
            .Build();

        var nodeA = graph.Define(new object(), "Path A")
            .Next(_ => endNode)
            .Build();

        var nodeB = graph.Define(new object(), "Path B")
            .Next(_ => endNode)
            .Build();

        var startNode = graph.Define(start, "Start")
            .Next(vm => vm.Choice == "A" ? nodeA : nodeB,
                canExecute: start.WhenAnyValue(x => x.Choice).Select(x => x is not null))
            .Build();

        var wizard = new GraphWizard<string>(startNode);
        var completion = wizard.Finished.ToTask();

        Assert.False(await wizard.Next.CanExecute.FirstAsync().ToTask());

        start.Choice = "A";
        await wizard.Next.CanExecute.FirstAsync(x => x).ToTask();

        await wizard.Next.Execute();
        Assert.Same(nodeA, wizard.CurrentStep);

        await wizard.Next.Execute();
        Assert.Same(endNode, wizard.CurrentStep);

        await wizard.Next.Execute();
        Assert.Equal("done", await completion);
    }

    [Fact]
    public async Task Typed_builder_supports_branching_to_path_b_and_finishes()
    {
        var start = new BranchModel();
        var graph = GraphWizardBuilder.For<string>();

        var endNode = graph.Define(new object(), "End")
            .Finish(_ => "done")
            .Build();

        var nodeA = graph.Define(new object(), "Path A")
            .Next(_ => endNode)
            .Build();

        var nodeB = graph.Define(new object(), "Path B")
            .Next(_ => endNode)
            .Build();

        var startNode = graph.Define(start, "Start")
            .Next(vm => vm.Choice == "A" ? nodeA : nodeB,
                canExecute: start.WhenAnyValue(x => x.Choice).Select(x => x is not null))
            .Build();

        var wizard = new GraphWizard<string>(startNode);
        var completion = wizard.Finished.ToTask();

        start.Choice = "B";
        await wizard.Next.CanExecute.FirstAsync(x => x).ToTask();

        await wizard.Next.Execute();
        Assert.Same(nodeB, wizard.CurrentStep);

        await wizard.Next.Execute();
        Assert.Same(endNode, wizard.CurrentStep);

        await wizard.Next.Execute();
        Assert.Equal("done", await completion);
    }

    private sealed record StepModel(string Value);

    private sealed class BranchModel : ReactiveObject
    {
        private string? choice;

        public string? Choice
        {
            get => choice;
            set => this.RaiseAndSetIfChanged(ref choice, value);
        }
    }
}