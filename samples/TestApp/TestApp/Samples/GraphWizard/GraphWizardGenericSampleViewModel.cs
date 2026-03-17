using System.Reactive;
using System.Reactive.Linq;
using CSharpFunctionalExtensions;
using ReactiveUI;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.Wizards.Graph.Builder;
using Zafiro.Avalonia.Wizards.Graph.Core;
using Zafiro.Reactive;
using Zafiro.UI;
using Zafiro.UI.Navigation;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.GraphWizard;

[Section("Generic Graph Wizard", "mdi-graph-outline", 3)]
[SectionGroup("navigation", "Navigation & Dialogs")]
public class GraphWizardGenericSampleViewModel : ReactiveObject
{
    private readonly IDialog dialog;
    private readonly INavigator navigator;
    private readonly INotificationService notificationService;

    public GraphWizardGenericSampleViewModel(INavigator navigator, IDialog dialog,
        INotificationService notificationService)
    {
        this.navigator = navigator;
        this.dialog = dialog;
        this.notificationService = notificationService;

        NavigateToWizard = ReactiveCommand.CreateFromTask(async () =>
        {
            var wizard = CreateGenericWizard();
            var result = await wizard.Navigate(navigator);

            await result.Match(
                value => notificationService.Show($"Wizard finished with result: {value}", "Info"),
                () => notificationService.Show("Wizard cancelled", "Info"));
        });

        ShowWizardInDialog = ReactiveCommand.CreateFromTask(async () =>
        {
            var wizard = CreateGenericWizard();
            var result = await wizard.ShowInDialog(dialog, "Generic Graph Wizard");

            await result.Match(
                value => notificationService.Show($"Dialog wizard finished with result: {value}", "Info"),
                () => notificationService.Show("Dialog wizard cancelled", "Info"));
        });
    }

    public ReactiveCommand<Unit, Unit> NavigateToWizard { get; }
    public ReactiveCommand<Unit, Unit> ShowWizardInDialog { get; }

    private GraphWizard<string> CreateGenericWizard()
    {
        var graph = GraphWizardBuilder.For<string>();
        var step1 = new Step1ViewModel();
        var step2A = new GenericStepViewModel("You selected Option A. Click Finish to return 'A'.");
        var step2B = new GenericStepViewModel("You selected Option B. Click Finish to return 'B'.");

        var node2A = graph
            .Define(step2A, "Option A")
            .Finish(vm => "A", nextLabel: "Finish (Return A)")
            .Build();

        var node2B = graph
            .Define(step2B, "Option B")
            .Finish(vm => "B", nextLabel: "Finish (Return B)")
            .Build();

        var dynamicLabel = step1.WhenAnyValue(x => x.Choice)
            .Select(c => c switch
            {
                "A" => "Proceed with A",
                "B" => "Proceed with B",
                _ => "Select an option"
            });

        var node1 = graph
            .Define(step1, "Selection")
            .Next(vm => vm.Choice == "A" ? node2A : node2B,
                canExecute: step1.WhenAnyValue(x => x.Choice).NotNull(),
                nextLabel: dynamicLabel)
            .Build();

        return new GraphWizard<string>(node1);
    }
}