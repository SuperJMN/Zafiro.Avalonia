using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ReactiveUI;
using TestApp.Samples.SlimWizard.Pages;
using TestApp.Samples.SlimWizard.Subwizard.Pages;
using Zafiro.Avalonia.Controls.Wizards.Slim;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.Dialogs.Wizards.Slim;
using Zafiro.Mixins;
using Zafiro.UI;
using Zafiro.UI.Commands;
using Zafiro.UI.Navigation;
using Zafiro.UI.Shell.Utils;
using Zafiro.UI.Wizards.Slim;
using Zafiro.UI.Wizards.Slim.Builder;

namespace TestApp.Samples.SlimWizard;

[Section("Wizard", "mdi-wizard-hat", 1)]
[SectionGroup("navigation", "Navigation & Dialogs")]
public class WizardViewModel : IDisposable
{
    private readonly CompositeDisposable disposable = new();
    private readonly INotificationService notification;

    public WizardViewModel(IDialog dialog, INotificationService notification, INavigator navigator)
    {
        this.notification = notification;

        ShowWizardInDialog = ReactiveCommand.CreateFromTask(() => CreateWizard().ShowInDialog(dialog, "This is a tasty wizard"));
        NavigateToWizard = ReactiveCommand.CreateFromTask(async () =>
        {
            var wizard = CreateWizard();
            var cancel = ReactiveCommand.CreateFromTask(() => navigator.GoBack());
            var host = new NavigationWizardHost(wizard, cancel.Enhance("Cancel", "Cancel"));

            await navigator.Go(() => host);

            var result = await wizard.Finished.Select(Maybe.From).FirstOrDefaultAsync();
            await navigator.GoBack();

            return result;
        });
        NavigateToWizardWithSubwizard = ReactiveCommand.CreateFromTask(() => CreateWizardWithSubwizard(navigator).Navigate(navigator));

        NavigateToWizard.Merge(ShowWizardInDialog)
            .SelectMany(maybe => ShowResults(maybe).ToSignal())
            .Subscribe()
            .DisposeWith(disposable);

        NavigateToWizardWithSubwizard
            .SelectMany(maybe => ShowSubwizardResults(maybe).ToSignal())
            .Subscribe()
            .DisposeWith(disposable);
    }

    public ReactiveCommand<Unit, Maybe<(int result, string)>> NavigateToWizard { get; set; }

    public ReactiveCommand<Unit, Maybe<(int result, string)>> ShowWizardInDialog { get; }

    public ReactiveCommand<Unit, Maybe<string>> NavigateToWizardWithSubwizard { get; }

    public void Dispose()
    {
        NavigateToWizard.Dispose();
        ShowWizardInDialog.Dispose();
        NavigateToWizardWithSubwizard.Dispose();
    }

    private Task ShowResults(Maybe<(int result, string)> maybe)
    {
        var message = maybe.Match(value => $"This is the data we gathered from it: '{value}'", () => "We got nothing, because the wizard was cancelled");
        return notification.Show(message, "Finished");
    }

    private Task ShowSubwizardResults(Maybe<string> maybe)
    {
        var message = maybe.Match(
            value => $"Wizard completed with: '{value}'",
            () => "We got nothing, because the wizard (or a child wizard) was cancelled");

        return notification.Show(message, "Finished");
    }

    private static SlimWizard<(int result, string)> CreateWizard()
    {
        var withCompletionFinalStep = WizardBuilder
            // Page1ViewModel implements IHaveTitle: if no title is provided, its reactive Title is used
            .StartWith(() => new Page1ViewModel(), "Page 1").NextWith(model => model.ReturnSomeInt.Enhance("Next"))
            // Page2ViewModel also implements IHaveTitle, so its Title is used by default
            .Then(number => new Page2ViewModel(number)).Next((vm, number) => (result: number, vm.Text!)).WhenValid()
            // Last page with an explicit static title
            .Then(_ => new Page3ViewModel(), "Completed!").Next((_, val) => val, "Close").WhenValid()
            .WithCompletionFinalStep();

        return withCompletionFinalStep;
    }

    private static SlimWizard<string> CreateWizardWithSubwizard(INavigator navigator)
    {
        Task<Maybe<string>> StartChildWizardA()
        {
            return CreateChildWizard("A").Navigate(navigator);
        }

        Task<Maybe<string>> StartChildWizardB()
        {
            return CreateChildWizard("B").Navigate(navigator);
        }

        return WizardBuilder
            .StartWith(() => new SubwizardWelcomePageViewModel(), "Wizard with subwizard")
            .NextUnit("Start").Always()
            .Then(_ => new SubwizardDecisionPageViewModel(StartChildWizardA, StartChildWizardB), "Choose a child wizard")
            .NextCommand(vm => vm.RunSelectedChildWizard)
            .Then(childResult => new SubwizardSummaryPageViewModel(childResult), "Summary")
            .Next((_, childResult) => childResult, "Finish").Always()
            .WithCompletionFinalStep();
    }

    private static SlimWizard<string> CreateChildWizard(string kind)
    {
        return WizardBuilder
            .StartWith(() => new ChildWizardInputPageViewModel(kind), $"Child wizard {kind}")
            .Next(vm => vm.Result!, "Finish").WhenValid()
            .WithCommitFinalStep();
    }
}