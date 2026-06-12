using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Wizards.Graph.Builder.Fluent;

public interface IGraphFlowBuilder<TResult>
{
    IFlowStepBuilder<TModel, TResult> StartWith<TModel>(Func<TModel> modelFactory, string title);
    IFlowStepBuilder<TModel, TResult> StartWith<TModel>(Func<TModel> modelFactory, IObservable<string> title);
}

public interface IFlowStepBuilder<TModel, TResult>
{
    IFlowStepBuilder<TNextModel, TResult> Step<TNextModel>(Func<TNextModel> modelFactory, string title, Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null);
    IFlowStepBuilder<TNextModel, TResult> Step<TNextModel>(Func<TNextModel> modelFactory, IObservable<string> title, Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null);

    // Branching -> returns a builder that can only Build(), effectively ending the linear chain
    IFlowStepBuilder<TResult> Branch<TProp>(Func<TModel, TProp> selector, Action<IBranchBuilder<TProp, TResult>> config, Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null) where TProp : notnull;

    // Finishing -> Returns the final step definition directly
    IWizardStep<TResult> Finish(Func<TModel, TResult> resultSelector, Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null);
    IWizardStep<TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector, Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null);
}

public interface IFlowStepBuilder<TResult>
{
    IWizardStep<TResult> Build();
}

public interface IBranchBuilder<TProp, TResult>
{
    void Case(TProp value, Func<IGraphFlowBuilder<TResult>, IWizardStep<TResult>> flowConfig);
}
