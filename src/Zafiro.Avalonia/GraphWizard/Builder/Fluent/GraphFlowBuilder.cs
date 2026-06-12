using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Wizards.Graph.Builder.Fluent;

public class GraphFlowBuilder<TResult> : IGraphFlowBuilder<TResult>
{
    public IFlowStepBuilder<TModel, TResult> StartWith<TModel>(Func<TModel> modelFactory, string title)
    {
        var root = new FlowNodeBuilder<TModel, TResult>(modelFactory, Observable.Return(title));
        return new FlowChain<TModel, TResult>(root, root);
    }

    public IFlowStepBuilder<TModel, TResult> StartWith<TModel>(Func<TModel> modelFactory, IObservable<string> title)
    {
        var root = new FlowNodeBuilder<TModel, TResult>(modelFactory, title);
        return new FlowChain<TModel, TResult>(root, root);
    }

    public static IGraphFlowBuilder<TResult> New() => new GraphFlowBuilder<TResult>();
}

internal class FlowChain<TModel, TResult> : IFlowStepBuilder<TModel, TResult>
{
    private readonly FlowNodeBuilder<TModel, TResult> current;
    private readonly IBuilder<TResult> root;

    public FlowChain(IBuilder<TResult> root, FlowNodeBuilder<TModel, TResult> current)
    {
        this.root = root;
        this.current = current;
    }

    public IFlowStepBuilder<TNextModel, TResult> Step<TNextModel>(Func<TNextModel> modelFactory, string title,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
    {
        return Step(modelFactory, Observable.Return(title), canExecute, nextLabel);
    }

    public IFlowStepBuilder<TNextModel, TResult> Step<TNextModel>(Func<TNextModel> modelFactory, IObservable<string> title,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
    {
        var nextNodeBuilder = current.Step(modelFactory, title, canExecute, nextLabel);
        return new FlowChain<TNextModel, TResult>(root, nextNodeBuilder);
    }

    public IFlowStepBuilder<TResult> Branch<TProp>(Func<TModel, TProp> selector,
        Action<IBranchBuilder<TProp, TResult>> config, Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
        where TProp : notnull
    {
        current.Branch(selector, config, canExecute, nextLabel);
        return new FlowChainTerminal<TResult>(root);
    }

    public IWizardStep<TResult> Finish(Func<TModel, TResult> resultSelector, Func<TModel, IObservable<bool>>? canExecute = null,
        string? nextLabel = null)
    {
        current.Finish(resultSelector, canExecute, nextLabel);
        return root.Build();
    }

    public IWizardStep<TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
    {
        current.Finish(resultSelector, canExecute, nextLabel);
        return root.Build();
    }
}

internal class FlowChainTerminal<TResult> : IFlowStepBuilder<TResult>
{
    private readonly IBuilder<TResult> root;

    public FlowChainTerminal(IBuilder<TResult> root)
    {
        this.root = root;
    }

    public IWizardStep<TResult> Build()
    {
        return root.Build();
    }
}

internal class FlowNodeBuilder<TModel, TResult> : IBuilder<TResult>
{
    private readonly Func<TModel> modelFactory;
    private readonly IObservable<string> title;
    private Func<IWizardStep<TResult>>? nextStepFactory;

    public FlowNodeBuilder(Func<TModel> modelFactory, IObservable<string> title)
    {
        this.modelFactory = modelFactory;
        this.title = title;
    }

    public IWizardStep<TResult> Build()
    {
        if (nextStepFactory == null)
        {
            throw new InvalidOperationException(
                $"Flow ending at {typeof(TModel).Name} is incomplete. You must call Finish() or point to another Step().");
        }

        return nextStepFactory();
    }

    public FlowNodeBuilder<TNextModel, TResult> Step<TNextModel>(Func<TNextModel> nextModelFactory, IObservable<string> nextTitle,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
    {
        var nextStepBuilder = new FlowNodeBuilder<TNextModel, TResult>(nextModelFactory, nextTitle);

        this.nextStepFactory = () =>
        {
            var nextStep = nextStepBuilder.Build();
            return TypedWizardStepBuilder
                .Step<TModel, TResult>(modelFactory, title)
                .Next(_ => nextStep, GetValidationCanExecute(canExecute), nextLabel)
                .Build();
        };

        return nextStepBuilder;
    }

    public void Branch<TProp>(Func<TModel, TProp> selector, Action<IBranchBuilder<TProp, TResult>> config,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null) where TProp : notnull
    {
        var branchBuilder = new BranchBuilder<TProp, TResult>();
        config(branchBuilder);

        this.nextStepFactory = () =>
        {
            return TypedWizardStepBuilder
                .Step<TModel, TResult>(modelFactory, title)
                .Next(m =>
                {
                    var prop = selector(m);
                    return branchBuilder.GetNode(prop);
                }, GetValidationCanExecute(canExecute), nextLabel)
                .Build();
        };
    }

    public void Finish(Func<TModel, TResult> resultSelector, Func<TModel, IObservable<bool>>? canExecute = null,
        string? nextLabel = null)
    {
        this.nextStepFactory = () =>
        {
            return TypedWizardStepBuilder
                .Step<TModel, TResult>(modelFactory, title)
                .Finish(resultSelector, GetValidationCanExecute(canExecute), nextLabel)
                .Build();
        };
    }

    public void Finish(Func<TModel, Task<Result<TResult>>> resultSelector, Func<TModel, IObservable<bool>>? canExecute = null,
        string? nextLabel = null)
    {
        this.nextStepFactory = () =>
        {
            return TypedWizardStepBuilder
                .Step<TModel, TResult>(modelFactory, title)
                .Finish(resultSelector, GetValidationCanExecute(canExecute), nextLabel)
                .Build();
        };
    }

    private static Func<TModel, IObservable<bool>> GetValidationCanExecute(Func<TModel, IObservable<bool>>? explicitCanExecute)
    {
        return model =>
        {
            var validationCanExecute = Observable.Return(true);
            if (model != null)
            {
                var isValidProp = model.GetType().GetProperty("IsValid");
                if (isValidProp != null && typeof(IObservable<bool>).IsAssignableFrom(isValidProp.PropertyType))
                {
                    if (isValidProp.GetValue(model) is IObservable<bool> val)
                    {
                        validationCanExecute = val;
                    }
                }
            }

            return explicitCanExecute != null
                ? explicitCanExecute(model).CombineLatest(validationCanExecute, (a, b) => a && b)
                : validationCanExecute;
        };
    }
}

internal interface IBuilder<TResult>
{
    IWizardStep<TResult> Build();
}

public class BranchBuilder<TProp, TResult> : IBranchBuilder<TProp, TResult> where TProp : notnull
{
    private readonly Dictionary<TProp, Func<IWizardStep<TResult>>> branches = new();

    public void Case(TProp value, Func<IGraphFlowBuilder<TResult>, IWizardStep<TResult>> flowConfig)
    {
        branches[value] = () => flowConfig(GraphFlowBuilder<TResult>.New());
    }

    public IWizardStep<TResult>? GetNode(TProp value)
    {
        if (branches.TryGetValue(value, out var factory))
        {
            return factory();
        }

        return null;
    }
}
