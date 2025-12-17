using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Zafiro.UI;
using Zafiro.UI.Commands;

namespace TestApp.Samples.SlimWizard.Subwizard.Pages;

public partial class SubwizardDecisionPageViewModel : ReactiveObject, IHaveTitle
{
    private readonly Func<Task<Maybe<string>>> startChildWizardA;
    private readonly Func<Task<Maybe<string>>> startChildWizardB;

    [Reactive] private bool useChildWizardA;
    [Reactive] private bool useChildWizardB;

    public SubwizardDecisionPageViewModel(
        Func<Task<Maybe<string>>> startChildWizardA,
        Func<Task<Maybe<string>>> startChildWizardB)
    {
        this.startChildWizardA = startChildWizardA;
        this.startChildWizardB = startChildWizardB;

        var canExecute = this.WhenAnyValue(x => x.UseChildWizardA, x => x.UseChildWizardB, (a, b) => a || b);

        RunSelectedChildWizard = ReactiveCommand.CreateFromTask(RunAsync, canExecute)
            .Enhance(text: "Run child wizard");

        Title = this.WhenAnyValue(x => x.UseChildWizardA, x => x.UseChildWizardB, (a, b) =>
        {
            if (a)
            {
                return "Choose: Child wizard A";
            }

            if (b)
            {
                return "Choose: Child wizard B";
            }

            return "Choose a child wizard";
        });
    }

    public IEnhancedCommand<Result<string>> RunSelectedChildWizard { get; }

    public IObservable<string> Title { get; }

    private async Task<Result<string>> RunAsync()
    {
        var start = UseChildWizardA ? startChildWizardA : startChildWizardB;
        var maybe = await start();

        return maybe.HasValue
            ? Result.Success(maybe.Value)
            : Result.Failure<string>("Child wizard cancelled");
    }
}