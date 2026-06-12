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

        // Steps are declared with content factories: a fresh view model is created on every entry.

        // 1. Define End step (typed)
        var endNode = graph.Step(() => new GenericStepViewModel("Finished!"), "End")
            .Finish(vm => "Done", nextLabel: "Finish!")
            .Build();

        // 2. Define Branch B (typed)
        var nodeB = graph.Step(() => new GenericStepViewModel("You chose path B"), "Path B")
            .Next(vm => endNode, nextLabel: "Complete B")
            .Build();

        // 3. Define Branch A (typed)
        var nodeA = graph.Step(() => new GenericStepViewModel("You chose path A"), "Path A")
            .Next(vm => endNode, nextLabel: "Complete A")
            .Build();

        // 4. Define Start step with typed result; guard and dynamic label bind to the fresh content.
        var startNode = graph.Step(() => new Step1ViewModel(), "Start")
            .Next(vm => vm.Choice == "A" ? nodeA : nodeB,
                canExecute: vm => vm.WhenAnyValue(x => x.Choice).NotNull(),
                nextLabel: vm => vm.WhenAnyValue(x => x.Choice)
                    .Select(choice => choice switch
                    {
                        "A" => "Choose A",
                        "B" => "Choose B",
                        _ => "Choose (Select logic)"
                    }))
            .Build();

        return new GraphWizard<string>(startNode);
    }
}