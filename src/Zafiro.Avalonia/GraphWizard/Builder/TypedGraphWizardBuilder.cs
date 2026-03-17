using Zafiro.Avalonia.Wizards.Graph.Builder.Fluent;

namespace Zafiro.Avalonia.Wizards.Graph.Builder;

/// <summary>
/// Typed entry point for building graph wizards without repeating the result type on every node.
/// </summary>
/// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
public sealed class TypedGraphWizardBuilder<TResult>
{
    /// <summary>
    /// Starts building a typed wizard node with a static title.
    /// </summary>
    public NodeBuilder<TModel, TResult> Define<TModel>(TModel model, string title)
    {
        return GraphWizardBuilderGeneric.Define<TModel, TResult>(model, title);
    }

    /// <summary>
    /// Starts building a typed wizard node with a dynamic title.
    /// </summary>
    public NodeBuilder<TModel, TResult> Define<TModel>(TModel model, IObservable<string> title)
    {
        return GraphWizardBuilderGeneric.Define<TModel, TResult>(model, title);
    }

    /// <summary>
    /// Starts building a typed graph flow where the result type is fixed for the whole flow.
    /// </summary>
    public IFlowStepBuilder<TModel, TResult> StartWith<TModel>(TModel model, string title)
    {
        return GraphFlowBuilder<TResult>.New().StartWith(model, title);
    }

    /// <summary>
    /// Starts building a typed graph flow with a dynamic title.
    /// </summary>
    public IFlowStepBuilder<TModel, TResult> StartWith<TModel>(TModel model, IObservable<string> title)
    {
        return GraphFlowBuilder<TResult>.New().StartWith(model, title);
    }
}