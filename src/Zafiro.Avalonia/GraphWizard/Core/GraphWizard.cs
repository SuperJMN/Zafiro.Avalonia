using System.Reactive;
using System.Reactive.Subjects;
using Zafiro.Avalonia.Wizards.Graph.Builder;
using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// Base for graph-based wizards. Holds the shared shell (current step, Next/Back/Cancel
/// commands, finish/title observables) while the concrete, typed <see cref="GraphWizard{TResult}"/>
/// owns navigation: it activates a fresh node on every entry and keeps its own back-stack of
/// step definitions.
/// </summary>
public class GraphWizard : ReactiveObject, IHaveHeader, IHaveFooter, IGraphWizard
{
    protected readonly Subject<Unit> FinishedBase = new();
    private IBaseWizardNode? currentStep;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphWizard"/> class.
    /// </summary>
    /// <param name="initialNode">The starting (already activated) node of the wizard flow.</param>
    /// <param name="nextTitle">
    /// Optional custom title for the Next button. Can be a static title or a dynamic observable.
    /// Defaults to "Next" if not specified.
    /// </param>
    public GraphWizard(IBaseWizardNode initialNode, IObservable<string>? nextTitle = null)
    {
        CurrentStep = initialNode;
        var canGoBack = this.WhenAnyValue(x => x.CurrentStep)
            .Select(_ => CanGoBack);

        Back = ReactiveCommand.Create(GoBackCore, canGoBack);
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

    /// <summary>
    /// Creates a typed graph wizard flow where the result type is fixed once for the whole wizard.
    /// </summary>
    /// <example>
    /// <code>
    /// var flow = GraphWizard.For&lt;string&gt;();
    /// var start = flow.Step(() => new StepViewModel(state), "Start")
    ///     .Next(_ => nextStep)
    ///     .Build();
    /// </code>
    /// </example>
    public static GraphWizardFlow<TResult> For<TResult>()
    {
        return new GraphWizardFlow<TResult>();
    }

    protected virtual void OnCancel() => FinishedBase.OnNext(Unit.Default);

    /// <summary>
    /// Creates the Next command. The base wizard cannot navigate on its own (it has no
    /// notion of step definitions); the typed wizard overrides this with real navigation.
    /// </summary>
    protected virtual ReactiveCommand<Unit, Unit> CreateNextCommand()
    {
        return ReactiveCommand.Create(() => { }, Observable.Return(false));
    }

    /// <summary>
    /// Navigates back to the previous step in the wizard.
    /// Called internally by the <see cref="Back"/> command.
    /// </summary>
    public void GoBack() => GoBackCore();

    /// <summary>
    /// Whether back navigation is currently possible. The typed wizard overrides this to
    /// report against its own back-stack of step definitions.
    /// </summary>
    protected virtual bool CanGoBack => false;

    /// <summary>
    /// Performs the back navigation. The typed wizard overrides this to re-activate the
    /// previous step (creating a fresh node).
    /// </summary>
    protected virtual void GoBackCore()
    {
    }
}
