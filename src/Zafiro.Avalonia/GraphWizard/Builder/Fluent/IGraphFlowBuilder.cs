using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Wizards.Graph.Builder.Fluent;

public interface IGraphFlowBuilder<TResult>
{
    IFlowStepBuilder<TModel, TResult> StartWith<TModel>(TModel model, string title);
    IFlowStepBuilder<TModel, TResult> StartWith<TModel>(TModel model, IObservable<string> title);
}

public interface IFlowStepBuilder<TModel, TResult>
{
    IFlowStepBuilder<TNextModel, TResult> Step<TNextModel>(TNextModel model, string title, IObservable<bool>? canExecute = null, string? nextLabel = null);
    IFlowStepBuilder<TNextModel, TResult> Step<TNextModel>(TNextModel model, IObservable<string> title, IObservable<bool>? canExecute = null, string? nextLabel = null);

    // Branching -> returns a builder that can only Build(), effectively ending the linear chain
    IFlowStepBuilder<TResult> Branch<TProp>(Func<TModel, TProp> selector, Action<IBranchBuilder<TProp, TResult>> config, IObservable<bool>? canExecute = null, string? nextLabel = null) where TProp : notnull;

    // Finishing -> Returns the final node directly
    IWizardNode<TResult> Finish(Func<TModel, TResult> resultSelector, IObservable<bool>? canExecute = null, string? nextLabel = null);
    IWizardNode<TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector, IObservable<bool>? canExecute = null, string? nextLabel = null);
}

public interface IFlowStepBuilder<TResult>
{
    IWizardNode<TResult> Build();
}

public interface IBranchBuilder<TProp, TResult>
{
    void Case(TProp value, Func<IGraphFlowBuilder<TResult>, IWizardNode<TResult>> flowConfig);
}