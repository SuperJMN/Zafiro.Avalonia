using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Wizards.Graph.Builder.Fluent;
using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Wizards.Graph.Builder;

/// <summary>
/// Primary entry point for building typed graph wizards.
/// Fixes the result type once and exposes step-based APIs for branching or linear flows.
/// <para>
/// Steps are declared with a content factory (<c>() =&gt; new TModel(...)</c>) so the wizard can
/// create a fresh, short-lived view-model on every entry. State is hoisted into whatever the
/// factory closes over (e.g. a shared session), never held by the step view-model across visits.
/// </para>
/// </summary>
/// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
public sealed class GraphWizardFlow<TResult>
{
    /// <summary>
    /// Starts building a wizard step with a static title.
    /// </summary>
    /// <example>
    /// <code>
    /// var flow = GraphWizard.For&lt;string&gt;();
    /// var start = flow.Step(() =&gt; new MyViewModel(state), "Start")
    ///     .Next(_ =&gt; nextStep)
    ///     .Build();
    /// </code>
    /// </example>
    public WizardStepBuilder<TModel, TResult> Step<TModel>(Func<TModel> modelFactory, string title)
    {
        return new WizardStepBuilder<TModel, TResult>(modelFactory, title);
    }

    /// <summary>
    /// Starts building a wizard step with a content-independent dynamic title.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Step<TModel>(Func<TModel> modelFactory, IObservable<string> title)
    {
        return new WizardStepBuilder<TModel, TResult>(modelFactory, title);
    }

    /// <summary>
    /// Starts building a wizard step with a title derived from the freshly created content.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Step<TModel>(Func<TModel> modelFactory, Func<TModel, IObservable<string>> title)
    {
        return new WizardStepBuilder<TModel, TResult>(modelFactory, title);
    }

    /// <summary>
    /// Starts building a linear typed flow from the first step.
    /// </summary>
    public IFlowStepBuilder<TModel, TResult> StartWith<TModel>(Func<TModel> modelFactory, string title)
    {
        return GraphFlowBuilder<TResult>.New().StartWith(modelFactory, title);
    }

    /// <summary>
    /// Starts building a linear typed flow from the first step with a dynamic title.
    /// </summary>
    public IFlowStepBuilder<TModel, TResult> StartWith<TModel>(Func<TModel> modelFactory, IObservable<string> title)
    {
        return GraphFlowBuilder<TResult>.New().StartWith(modelFactory, title);
    }
}

/// <summary>
/// Fluent builder for a typed wizard step.
/// Use <see cref="Next(Func{TModel, IWizardStep{TResult}?}, Func{TModel, IObservable{bool}}?, string?)"/> to continue to another step,
/// or <see cref="Finish(Func{TModel, TResult}, Func{TModel, IObservable{bool}}?, string?)"/> to complete the wizard with a result.
/// </summary>
/// <typeparam name="TModel">The type of content shown in the step.</typeparam>
/// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
public sealed class WizardStepBuilder<TModel, TResult>
{
    private readonly TypedWizardStepBuilderCore<TModel, TResult> inner;

    internal WizardStepBuilder(Func<TModel> modelFactory, string title)
    {
        inner = new TypedWizardStepBuilderCore<TModel, TResult>(modelFactory, _ => Observable.Return(title));
    }

    internal WizardStepBuilder(Func<TModel> modelFactory, IObservable<string> title)
    {
        inner = new TypedWizardStepBuilderCore<TModel, TResult>(modelFactory, _ => title);
    }

    internal WizardStepBuilder(Func<TModel> modelFactory, Func<TModel, IObservable<string>> title)
    {
        inner = new TypedWizardStepBuilderCore<TModel, TResult>(modelFactory, title);
    }

    /// <summary>
    /// Configures navigation to the next step.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Next(Func<TModel, IWizardStep<TResult>?> nextSelector,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
    {
        inner.Next(nextSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures navigation to the next step with a dynamic button label.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Next(Func<TModel, IWizardStep<TResult>?> nextSelector,
        Func<TModel, IObservable<bool>>? canExecute, Func<TModel, IObservable<string>> nextLabel)
    {
        inner.Next(nextSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures asynchronous navigation to the next step.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Next(Func<TModel, Task<Result<IWizardStep<TResult>?>>> nextSelector,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
    {
        inner.Next(nextSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures asynchronous navigation to the next step with a dynamic button label.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Next(Func<TModel, Task<Result<IWizardStep<TResult>?>>> nextSelector,
        Func<TModel, IObservable<bool>>? canExecute, Func<TModel, IObservable<string>> nextLabel)
    {
        inner.Next(nextSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures completion of the wizard with a typed result.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Finish(Func<TModel, TResult> resultSelector,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
    {
        inner.Finish(resultSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures completion of the wizard with a typed result and a dynamic button label.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Finish(Func<TModel, TResult> resultSelector,
        Func<TModel, IObservable<bool>>? canExecute, Func<TModel, IObservable<string>> nextLabel)
    {
        inner.Finish(resultSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures asynchronous completion of the wizard with a typed result.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
    {
        inner.Finish(resultSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures asynchronous completion of the wizard with a typed result and a dynamic button label.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector,
        Func<TModel, IObservable<bool>>? canExecute, Func<TModel, IObservable<string>> nextLabel)
    {
        inner.Finish(resultSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Builds the configured typed wizard step definition.
    /// </summary>
    public IWizardStep<TResult> Build()
    {
        return inner.Build();
    }
}
