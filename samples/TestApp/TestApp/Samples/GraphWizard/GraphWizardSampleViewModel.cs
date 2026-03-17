using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.Wizards.Graph.Core;
using Zafiro.Reactive;
using Zafiro.UI;
using Zafiro.UI.Navigation;
using Zafiro.UI.Shell.Utils;
using WizardGraph = Zafiro.Avalonia.Wizards.Graph.Core.GraphWizard;

namespace TestApp.Samples.GraphWizard;

[Section("Graph Wizard", "mdi-graph", 2)]
[SectionGroup("navigation", "Navigation & Dialogs")]
public class GraphWizardSampleViewModel : ReactiveObject
{
    public GraphWizardSampleViewModel(IDialog dialog, INavigator navigator, INotificationService notificationService)
    {
        NavigateToWizard = ReactiveCommand.CreateFromTask(async () =>
        {
            var wizard = CreateWizard();
            var result = await wizard.Navigate(navigator);
            if (result.HasValue)
            {
                await notificationService.Show($"Wizard result: {result.Value}", "Success");
            }
        });

        ShowWizardInDialog = ReactiveCommand.CreateFromTask(async () =>
        {
            var wizard = CreateWizard();
            var result = await wizard.ShowInDialog(dialog, "Graph Wizard");
            if (result.HasValue)
            {
                await notificationService.Show($"Wizard result from dialog: {result.Value}", "Success");
            }
        });
    }

    public ReactiveCommand<Unit, Unit> NavigateToWizard { get; }

    public ReactiveCommand<Unit, Unit> ShowWizardInDialog { get; }

    private static GraphWizard<string> CreateWizard()
    {
        var graph = WizardGraph.For<string>();

        // 1. Define End Nodes (typed)
        var end = new GenericStepViewModel("Finished!");
        var endNode = graph.Step(end, "End")
            .Finish(vm => "Done", nextLabel: "Finish!")
            .Build();

        // 2. Define Branch B (typed)
        var stepB = new GenericStepViewModel("You chose path B");
        var nodeB = graph.Step(stepB, "Path B")
            .Next(vm => endNode, nextLabel: "Complete B")
            .Build();

        // 3. Define Branch A (typed)
        var stepA = new GenericStepViewModel("You chose path A");
        var nodeA = graph.Step(stepA, "Path A")
            .Next(vm => endNode, nextLabel: "Complete A")
            .Build();

        // 4. Define Start Node with typed result
        var start = new Step1ViewModel();

        // Define dynamic label observable
        var dynamicLabel = start.WhenAnyValue(x => x.Choice)
            .Select(choice => choice switch
            {
                "A" => "Choose A",
                "B" => "Choose B",
                _ => "Choose (Select logic)"
            });

        var startNode = graph.Step(start, "Start")
            .Next(vm => vm.Choice == "A" ? nodeA : nodeB,
                canExecute: start.WhenAnyValue(x => x.Choice).NotNull(),
                nextLabel: dynamicLabel)
            .Build();

        return new GraphWizard<string>(startNode);
    }
}