using System.Reactive;
using System.Reactive.Subjects;
using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// A graph-based wizard that allows multi-step workflows with conditional branching.
/// Unlike linear wizards, each step can lead to different subsequent steps based on user choices.
/// </summary>
public class GraphWizard : ReactiveObject, IHaveHeader, IHaveFooter, IGraphWizard
{
    protected readonly Subject<Unit> FinishedBase = new();
    private readonly Stack<IBaseWizardNode> stack = new();
    private IBaseWizardNode? currentStep;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphWizard"/> class.
    /// </summary>
    /// <param name="initialNode">The starting node of the wizard flow.</param>
    /// <param name="nextTitle">
    /// Optional custom title for the Next button. Can be a static title or a dynamic observable.
    /// Defaults to "Next" if not specified.
    /// </param>
    public GraphWizard(IBaseWizardNode initialNode, IObservable<string>? nextTitle = null)
    {
        CurrentStep = initialNode;
        var canGoBack = this.WhenAnyValue(x => x.CurrentStep)
            .Select(_ => stack.Count > 0);

        Back = ReactiveCommand.Create(GoBack, canGoBack);
        Cancel = ReactiveCommand.Create(OnCancel);

        Next = CreateNextCommand();

        Finish = FinishedBase.AsObservable();

        NextTitle = nextTitle ?? this.WhenAnyValue(x => x.CurrentStep)
            .Select(x => x?.NextLabel ?? Observable.Return("Next"))
            .Switch();
    }

    /// <summary>
    /// Gets an observable that emits when the wizard completes or is cancelled.
    /// Subscribe to this to handle wizard completion (e.g., navigate away).
    /// </summary>
    public IObservable<Unit> Finish { get; }

    /// <summary>
    /// Gets the command to navigate back to the previous step.
    /// Automatically disabled when there are no previous steps.
    /// </summary>
    public ReactiveCommand<Unit, Unit> Back { get; }

    /// <summary>
    /// Gets the command to advance to the next step.
    /// Executes the current node's Next logic to determine the subsequent step.
    /// </summary>
    public ReactiveCommand<Unit, Unit> Next { get; }

    /// <summary>
    /// Gets the command to cancel the wizard.
    /// Triggers the <see cref="Finish"/> observable without completing the workflow.
    /// </summary>
    public ReactiveCommand<Unit, Unit> Cancel { get; }

    /// <summary>
    /// Gets an observable that emits the current title for the Next button.
    /// Can change dynamically based on the wizard configuration.
    /// </summary>
    public IObservable<string> NextTitle { get; }

    /// <summary>
    /// Gets the current wizard step being displayed.
    /// </summary>
    public IBaseWizardNode? CurrentStep
    {
        get => currentStep;
        protected set => this.RaiseAndSetIfChanged(ref currentStep, value);
    }

    /// <inheritdoc />
    IObservable<object> IHaveFooter.Footer => Observable.Return(new GraphWizardFooter(this)).Select(x => (object)x);

    /// <inheritdoc />
    IObservable<object> IHaveHeader.Header => Observable.Return(new GraphWizardHeader(this)).Select(x => (object)x);

    protected virtual void OnCancel() => FinishedBase.OnNext(Unit.Default);

    protected virtual ReactiveCommand<Unit, Unit> CreateNextCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            if (CurrentStep is not IWizardNode step) return;

            var result = await step.Next.Execute();
            if (result.IsSuccess)
            {
                if (result.Value != null)
                {
                    stack.Push(CurrentStep);
                    CurrentStep = result.Value;
                }
                else
                {
                    FinishedBase.OnNext(Unit.Default);
                }
            }
        }, this.WhenAnyValue(x => x.CurrentStep).SelectMany(x => (x as IWizardNode)?.Next.CanExecute ?? Observable.Return(false)));
    }

    /// <summary>
    /// Navigates back to the previous step in the wizard.
    /// Called internally by the <see cref="Back"/> command.
    /// </summary>
    public void GoBack()
    {
        if (stack.TryPop(out var previous))
        {
            CurrentStep = previous;
        }
    }

    protected void NavigateTo(IBaseWizardNode nextNode)
    {
        PushCurrent();
        CurrentStep = nextNode;
    }

    protected void PushCurrent()
    {
        if (CurrentStep != null)
        {
            stack.Push(CurrentStep);
        }
    }
}