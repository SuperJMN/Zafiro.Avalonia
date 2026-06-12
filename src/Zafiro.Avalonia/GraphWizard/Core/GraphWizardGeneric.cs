using System.Reactive;
using System.Reactive.Subjects;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// A graph-based wizard that allows multi-step workflows with conditional branching and returns a typed result.
/// Unlike linear wizards, each step can lead to different subsequent steps based on user choices.
/// Typical flows are created with <see cref="GraphWizard.For{TResult}"/> and then passed to this type.
/// <para>
/// Steps are <see cref="IWizardStep{TResult}"/> definitions; the wizard activates a fresh node
/// (with a newly created view-model) every time a step is entered, including on back navigation.
/// View models are therefore short-lived and never reused.
/// </para>
/// </summary>
/// <typeparam name="TResult">The type of the result that the wizard will produce upon completion.</typeparam>
public class GraphWizard<TResult> : GraphWizard
{
    private readonly Subject<TResult> finished = new();
    private readonly Stack<IWizardStep<TResult>> steps = new();
    private IWizardStep<TResult> currentStep;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphWizard{TResult}"/> class.
    /// </summary>
    /// <param name="initialStep">The starting step definition of the wizard flow.</param>
    /// <param name="nextTitle">
    /// Optional custom title for the Next button. Can be a static title or a dynamic observable.
    /// Defaults to "Next" if not specified.
    /// </param>
    public GraphWizard(IWizardStep<TResult> initialStep, IObservable<string>? nextTitle = null)
        : base(initialStep.CreateNode(), nextTitle)
    {
        currentStep = initialStep;
        Finished = finished.AsObservable();
    }

    /// <summary>
    /// Gets an observable that emits the result when the wizard completes successfully.
    /// Completes without emitting if the wizard is cancelled.
    /// </summary>
    public IObservable<TResult> Finished { get; }

    /// <inheritdoc />
    protected override bool CanGoBack => steps.Count > 0;

    protected override void OnCancel()
    {
        finished.OnCompleted();
        FinishedBase.OnNext(Unit.Default);
    }

    /// <inheritdoc />
    protected override void GoBackCore()
    {
        if (steps.TryPop(out var previous))
        {
            currentStep = previous;
            CurrentStep = previous.CreateNode();
        }
    }

    protected override ReactiveCommand<Unit, Unit> CreateNextCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
            {
                if (CurrentStep is not IWizardNode<TResult> step) return;

                var result = await step.Next.Execute();
                if (result.IsFailure) return;

                var wizardResult = result.Value;

                if (wizardResult.IsFinished)
                {
                    // Wizard completed with a result
                    finished.OnNext(wizardResult.Result);
                    finished.OnCompleted();
                    FinishedBase.OnNext(Unit.Default);
                }
                else if (wizardResult.NextStep != null)
                {
                    // Activate the next step with a fresh node/view-model
                    steps.Push(currentStep);
                    currentStep = wizardResult.NextStep;
                    CurrentStep = wizardResult.NextStep.CreateNode();
                }
            },
            this.WhenAnyValue(x => x.CurrentStep)
                .SelectMany(x => (x as IWizardNode<TResult>)?.Next.CanExecute ?? Observable.Return(false)));
    }
}

/// <summary>
/// Represents the result of wizard node navigation, either continuing to another step or finishing with a result.
/// </summary>
/// <typeparam name="TResult">The type of the wizard result.</typeparam>
public class WizardResult<TResult>
{
    private WizardResult(IWizardStep<TResult>? nextStep, TResult result, bool isFinished)
    {
        NextStep = nextStep;
        Result = result!;
        IsFinished = isFinished;
    }

    /// <summary>
    /// The next step to activate, or <see langword="null"/> when the wizard finishes.
    /// </summary>
    public IWizardStep<TResult>? NextStep { get; }

    public TResult Result { get; }
    public bool IsFinished { get; }

    public static WizardResult<TResult> Continue(IWizardStep<TResult> nextStep) => new(nextStep, default!, false);
    public static WizardResult<TResult> Complete(TResult result) => new(null, result, true);
}
