using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Wizards.Graph.Builder.Fluent;
using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Wizards.Graph.Builder;

/// <summary>
/// Primary entry point for building typed graph wizards.
/// Fixes the result type once and exposes step-based APIs for branching or linear flows.
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
    /// var start = flow.Step(viewModel, "Start")
    ///     .Next(_ => nextNode)
    ///     .Build();
    /// </code>
    /// </example>
    public WizardStepBuilder<TModel, TResult> Step<TModel>(TModel model, string title)
    {
        return new WizardStepBuilder<TModel, TResult>(model, title);
    }

    /// <summary>
    /// Starts building a wizard step with a dynamic title.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Step<TModel>(TModel model, IObservable<string> title)
    {
        return new WizardStepBuilder<TModel, TResult>(model, title);
    }

    /// <summary>
    /// Starts building a linear typed flow from the first step.
    /// </summary>
    public IFlowStepBuilder<TModel, TResult> StartWith<TModel>(TModel model, string title)
    {
        return GraphFlowBuilder<TResult>.New().StartWith(model, title);
    }

    /// <summary>
    /// Starts building a linear typed flow from the first step with a dynamic title.
    /// </summary>
    public IFlowStepBuilder<TModel, TResult> StartWith<TModel>(TModel model, IObservable<string> title)
    {
        return GraphFlowBuilder<TResult>.New().StartWith(model, title);
    }
}

/// <summary>
/// Fluent builder for a typed wizard step.
/// Use <see cref="Next(Func{TModel, IWizardNode{TResult}?}, IObservable{bool}?, string?)"/> to continue to another step,
/// or <see cref="Finish(Func{TModel, TResult}, IObservable{bool}?, string?)"/> to complete the wizard with a result.
/// </summary>
/// <typeparam name="TModel">The type of content shown in the step.</typeparam>
/// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
public sealed class WizardStepBuilder<TModel, TResult>
{
    private readonly TypedWizardStepBuilderCore<TModel, TResult> inner;

    internal WizardStepBuilder(TModel model, string title)
    {
        inner = new TypedWizardStepBuilderCore<TModel, TResult>(model, title);
    }

    internal WizardStepBuilder(TModel model, IObservable<string> title)
    {
        inner = new TypedWizardStepBuilderCore<TModel, TResult>(model, title);
    }

    /// <summary>
    /// Configures navigation to the next step.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Next(Func<TModel, IWizardNode<TResult>?> nextSelector,
        IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        inner.Next(nextSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures navigation to the next step with a dynamic button label.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Next(Func<TModel, IWizardNode<TResult>?> nextSelector,
        IObservable<bool>? canExecute, IObservable<string> nextLabel)
    {
        inner.Next(nextSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures asynchronous navigation to the next step.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Next(Func<TModel, Task<Result<IWizardNode<TResult>?>>> nextSelector,
        IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        inner.Next(nextSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures asynchronous navigation to the next step with a dynamic button label.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Next(Func<TModel, Task<Result<IWizardNode<TResult>?>>> nextSelector,
        IObservable<bool>? canExecute, IObservable<string> nextLabel)
    {
        inner.Next(nextSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures completion of the wizard with a typed result.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Finish(Func<TModel, TResult> resultSelector,
        IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        inner.Finish(resultSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures completion of the wizard with a typed result and a dynamic button label.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Finish(Func<TModel, TResult> resultSelector,
        IObservable<bool>? canExecute, IObservable<string> nextLabel)
    {
        inner.Finish(resultSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures asynchronous completion of the wizard with a typed result.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector,
        IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        inner.Finish(resultSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Configures asynchronous completion of the wizard with a typed result and a dynamic button label.
    /// </summary>
    public WizardStepBuilder<TModel, TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector,
        IObservable<bool>? canExecute, IObservable<string> nextLabel)
    {
        inner.Finish(resultSelector, canExecute, nextLabel);
        return this;
    }

    /// <summary>
    /// Builds the configured typed wizard node.
    /// </summary>
    public IWizardNode<TResult> Build()
    {
        return inner.Build();
    }
}