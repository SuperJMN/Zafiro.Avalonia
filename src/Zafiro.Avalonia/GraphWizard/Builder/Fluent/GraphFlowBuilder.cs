using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Wizards.Graph.Builder.Fluent;

public class GraphFlowBuilder<TResult> : IGraphFlowBuilder<TResult>
{
    public IFlowStepBuilder<TModel, TResult> StartWith<TModel>(TModel model, string title)
    {
        var root = new NodeBuilder<TModel, TResult>(model, Observable.Return(title));
        return new FlowChain<TModel, TResult>(root, root);
    }

    public IFlowStepBuilder<TModel, TResult> StartWith<TModel>(TModel model, IObservable<string> title)
    {
        var root = new NodeBuilder<TModel, TResult>(model, title);
        return new FlowChain<TModel, TResult>(root, root);
    }

    public static IGraphFlowBuilder<TResult> New() => new GraphFlowBuilder<TResult>();
}

internal class FlowChain<TModel, TResult> : IFlowStepBuilder<TModel, TResult>
{
    private readonly NodeBuilder<TModel, TResult> current;
    private readonly IBuilder<TResult> root;

    public FlowChain(IBuilder<TResult> root, NodeBuilder<TModel, TResult> current)
    {
        this.root = root;
        this.current = current;
    }

    public IFlowStepBuilder<TNextModel, TResult> Step<TNextModel>(TNextModel model, string title, IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        return Step(model, Observable.Return(title), canExecute, nextLabel);
    }

    public IFlowStepBuilder<TNextModel, TResult> Step<TNextModel>(TNextModel model, IObservable<string> title, IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        var nextNodeBuilder = current.Step(model, title, canExecute, nextLabel);
        return new FlowChain<TNextModel, TResult>(root, nextNodeBuilder);
    }

    public IFlowStepBuilder<TResult> Branch<TProp>(Func<TModel, TProp> selector, Action<IBranchBuilder<TProp, TResult>> config, IObservable<bool>? canExecute = null, string? nextLabel = null) where TProp : notnull
    {
        current.Branch(selector, config, canExecute, nextLabel);
        return new FlowChainTerminal<TResult>(root);
    }

    public IWizardNode<TResult> Finish(Func<TModel, TResult> resultSelector, IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        current.Finish(resultSelector, canExecute, nextLabel);
        return root.Build();
    }

    public IWizardNode<TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector, IObservable<bool>? canExecute = null, string? nextLabel = null)
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

    public IWizardNode<TResult> Build()
    {
        return root.Build();
    }
}

internal class NodeBuilder<TModel, TResult> : IBuilder<TResult>
{
    private readonly TModel model;
    private readonly IObservable<string> title;
    private Func<IWizardNode<TResult>>? nextNodeFactory;

    public NodeBuilder(TModel model, IObservable<string> title)
    {
        this.model = model;
        this.title = title;
    }

    public IWizardNode<TResult> Build()
    {
        if (nextNodeFactory == null)
        {
            throw new InvalidOperationException($"Flow ending at {model?.GetType().Name} is incomplete. You must call Finish() or point to another Step().");
        }

        return nextNodeFactory();
    }

    public NodeBuilder<TNextModel, TResult> Step<TNextModel>(TNextModel nextModel, IObservable<string> nextTitle, IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        var nextStepBuilder = new NodeBuilder<TNextModel, TResult>(nextModel, nextTitle);

        this.nextNodeFactory = () =>
        {
            var nextNode = nextStepBuilder.Build();
            return GraphWizardBuilderGeneric
                .Define<TModel, TResult>(model, title)
                .Next(_ => nextNode, GetValidationCanExecute(canExecute), nextLabel)
                .Build();
        };

        return nextStepBuilder;
    }

    public void Branch<TProp>(Func<TModel, TProp> selector, Action<IBranchBuilder<TProp, TResult>> config, IObservable<bool>? canExecute = null, string? nextLabel = null) where TProp : notnull
    {
        var branchBuilder = new BranchBuilder<TProp, TResult>();
        config(branchBuilder);

        this.nextNodeFactory = () =>
        {
            return GraphWizardBuilderGeneric
                .Define<TModel, TResult>(model, title)
                .Next(m =>
                {
                    var prop = selector(m);
                    return branchBuilder.GetNode(prop);
                }, GetValidationCanExecute(canExecute), nextLabel)
                .Build();
        };
    }

    public void Finish(Func<TModel, TResult> resultSelector, IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        this.nextNodeFactory = () =>
        {
            return GraphWizardBuilderGeneric
                .Define<TModel, TResult>(model, title)
                .Finish(resultSelector, GetValidationCanExecute(canExecute), nextLabel)
                .Build();
        };
    }

    public void Finish(Func<TModel, Task<Result<TResult>>> resultSelector, IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        this.nextNodeFactory = () =>
        {
            return GraphWizardBuilderGeneric
                .Define<TModel, TResult>(model, title)
                .Finish(resultSelector, GetValidationCanExecute(canExecute), nextLabel)
                .Build();
        };
    }

    private IObservable<bool> GetValidationCanExecute(IObservable<bool>? explicitCanExecute)
    {
        var validationCanExecute = Observable.Return(true);
        if (model != null)
        {
            var isValidProp = model.GetType().GetProperty("IsValid");
            if (isValidProp != null && typeof(IObservable<bool>).IsAssignableFrom(isValidProp.PropertyType))
            {
                var val = isValidProp.GetValue(model) as IObservable<bool>;
                if (val != null)
                {
                    validationCanExecute = val;
                }
            }
        }

        return explicitCanExecute != null
            ? explicitCanExecute.CombineLatest(validationCanExecute, (a, b) => a && b)
            : validationCanExecute;
    }
}

internal interface IBuilder<TResult>
{
    IWizardNode<TResult> Build();
}

public class BranchBuilder<TProp, TResult> : IBranchBuilder<TProp, TResult> where TProp : notnull
{
    private readonly Dictionary<TProp, Func<IWizardNode<TResult>>> branches = new();

    public void Case(TProp value, Func<IGraphFlowBuilder<TResult>, IWizardNode<TResult>> flowConfig)
    {
        branches[value] = () => flowConfig(GraphFlowBuilder<TResult>.New());
    }

    public IWizardNode<TResult>? GetNode(TProp value)
    {
        if (branches.TryGetValue(value, out var factory))
        {
            return factory();
        }

        return null;
    }
}