using System.Reactive;
using System.Reactive.Subjects;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// A graph-based wizard that allows multi-step workflows with conditional branching and returns a typed result.
/// Unlike linear wizards, each step can lead to different subsequent steps based on user choices.
/// Typical flows are created with <see cref="GraphWizard.For{TResult}"/> and then passed to this type.
/// </summary>
/// <typeparam name="TResult">The type of the result that the wizard will produce upon completion.</typeparam>
public class GraphWizard<TResult> : GraphWizard
{
    private readonly Subject<TResult> finished = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphWizard{TResult}"/> class.
    /// </summary>
    /// <param name="initialNode">The starting node of the wizard flow.</param>
    /// <param name="nextTitle">
    /// Optional custom title for the Next button. Can be a static title or a dynamic observable.
    /// Defaults to "Next" if not specified.
    /// </param>
    public GraphWizard(IWizardNode<TResult> initialNode, IObservable<string>? nextTitle = null) : base(initialNode,
        nextTitle)
    {
        Finished = finished.AsObservable();
    }

    /// <summary>
    /// Gets an observable that emits the result when the wizard completes successfully.
    /// Completes without emitting if the wizard is cancelled.
    /// </summary>
    public IObservable<TResult> Finished { get; }

    protected override void OnCancel()
    {
        finished.OnCompleted();
        FinishedBase.OnNext(Unit.Default);
    }

    protected override ReactiveCommand<Unit, Unit> CreateNextCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
            {
                if (CurrentStep is not IWizardNode<TResult> step) return;

                var result = await step.Next.Execute();
                if (result.IsSuccess)
                {
                    var wizardResult = result.Value;

                    if (wizardResult.IsFinished)
                    {
                        // Wizard completed with a result
                        finished.OnNext(wizardResult.Result);
                        finished.OnCompleted();
                        FinishedBase.OnNext(Unit.Default);
                    }
                    else if (wizardResult.NextNode != null)
                    {
                        // Navigate to next node
                        NavigateTo(wizardResult.NextNode);
                    }
                }
            },
            this.WhenAnyValue(x => x.CurrentStep)
                .SelectMany(x => (x as IWizardNode<TResult>)?.Next.CanExecute ?? Observable.Return(false)));
    }
}

/// <summary>
/// Represents the result of wizard node navigation, either continuing to another node or finishing with a result.
/// </summary>
/// <typeparam name="TResult">The type of the wizard result.</typeparam>
public class WizardResult<TResult>
{
    private WizardResult(IWizardNode<TResult>? nextNode, TResult result, bool isFinished)
    {
        NextNode = nextNode;
        Result = result!;
        IsFinished = isFinished;
    }

    public IWizardNode<TResult>? NextNode { get; }
    public TResult Result { get; }
    public bool IsFinished { get; }

    public static WizardResult<TResult> Continue(IWizardNode<TResult> nextNode) => new(nextNode, default!, false);
    public static WizardResult<TResult> Complete(TResult result) => new(null, result, true);
}