using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Wizards.Graph.Builder;

/// <summary>
/// Provides a fluent API for building wizard nodes with typed results.
/// </summary>
public static class GraphWizardBuilderGeneric
{
    /// <summary>
    /// Starts building a wizard node with a static title that will produce a typed result.
    /// </summary>
    /// <typeparam name="TModel">The type of the view model or content for this step.</typeparam>
    /// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
    /// <param name="model">The view model or content to display in this wizard step.</param>
    /// <param name="title">The static title for this step.</param>
    /// <returns>A <see cref="NodeBuilder{TModel,TResult}"/> to configure the node further.</returns>
    public static NodeBuilder<TModel, TResult> Define<TModel, TResult>(TModel model, string title)
    {
        return new NodeBuilder<TModel, TResult>(model, title);
    }

    /// <summary>
    /// Starts building a wizard node with a dynamic title that will produce a typed result.
    /// </summary>
    /// <typeparam name="TModel">The type of the view model or content for this step.</typeparam>
    /// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
    /// <param name="model">The view model or content to display in this wizard step.</param>
    /// <param name="title">An observable that emits the title for this step.</param>
    /// <returns>A <see cref="NodeBuilder{TModel,TResult}"/> to configure the node further.</returns>
    public static NodeBuilder<TModel, TResult> Define<TModel, TResult>(TModel model, IObservable<string> title)
    {
        return new NodeBuilder<TModel, TResult>(model, title);
    }
}

/// <summary>
/// Fluent builder for configuring wizard nodes with typed results.
/// </summary>
/// <typeparam name="TModel">The type of the view model or content for this node.</typeparam>
/// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
public class NodeBuilder<TModel, TResult>
{
    private readonly TModel model;
    private IObservable<bool> canNext = Observable.Return(true);

    private Func<TModel, Task<Result<WizardResult<TResult>>>> nextFactory = _ =>
        Task.FromResult(Result.Failure<WizardResult<TResult>>("Next not configured"));

    private IObservable<string>? nextLabel;
    private IObservable<string> title;

    public NodeBuilder(TModel model, string title)
    {
        this.model = model;
        this.title = Observable.Return(title);
    }

    public NodeBuilder(TModel model, IObservable<string> title)
    {
        this.model = model;
        this.title = title;
    }

    /// <summary>
    /// Builds and returns the configured node for a typed wizard.
    /// </summary>
    public IWizardNode<TResult> Build()
    {
        return new WizardNodeGeneric<TResult>(model!, title, () => nextFactory(model), canNext, nextLabel);
    }

    /// <summary>
    /// Defines navigation to the next node in the wizard.
    /// </summary>
    /// <param name="nextSelector">Function that returns the next node.</param>
    /// <param name="canExecute">Optional observable controlling when Next can execute.</param>
    /// <param name="nextLabel">Optional label for the Next button.</param>
    public NodeBuilder<TModel, TResult> Next(Func<TModel, IWizardNode<TResult>?> nextSelector, IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        this.nextFactory = m =>
        {
            var next = nextSelector(m);
            return Task.FromResult(next != null
                ? Result.Success(WizardResult<TResult>.Continue(next))
                : Result.Failure<WizardResult<TResult>>("No next node"));
        };

        if (canExecute != null)
        {
            this.canNext = canExecute;
        }

        if (nextLabel != null)
        {
            this.nextLabel = Observable.Return(nextLabel);
        }

        return this;
    }

    /// <summary>
    /// Defines navigation to the next node in the wizard with a dynamic Next button label.
    /// </summary>
    public NodeBuilder<TModel, TResult> Next(Func<TModel, IWizardNode<TResult>?> nextSelector, IObservable<bool>? canExecute, IObservable<string> nextLabel)
    {
        this.nextFactory = m =>
        {
            var next = nextSelector(m);
            return Task.FromResult(next != null
                ? Result.Success(WizardResult<TResult>.Continue(next))
                : Result.Failure<WizardResult<TResult>>("No next node"));
        };

        if (canExecute != null)
        {
            this.canNext = canExecute;
        }

        this.nextLabel = nextLabel;

        return this;
    }

    /// <summary>
    /// Defines asynchronous navigation to the next node in the wizard.
    /// </summary>
    /// <param name="nextSelector">Async function that returns the next node.</param>
    /// <param name="canExecute">Optional observable controlling when Next can execute.</param>
    /// <param name="nextLabel">Optional label for the Next button.</param>
    public NodeBuilder<TModel, TResult> Next(Func<TModel, Task<Result<IWizardNode<TResult>?>>> nextSelector, IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        this.nextFactory = async m =>
        {
            var result = await nextSelector(m);
            if (result.IsFailure)
            {
                return Result.Failure<WizardResult<TResult>>(result.Error);
            }

            return result.Value != null
                ? Result.Success(WizardResult<TResult>.Continue(result.Value))
                : Result.Failure<WizardResult<TResult>>("No next node");
        };

        if (canExecute != null)
        {
            this.canNext = canExecute;
        }

        if (nextLabel != null)
        {
            this.nextLabel = Observable.Return(nextLabel);
        }

        return this;
    }

    /// <summary>
    /// Defines asynchronous navigation to the next node in the wizard with a dynamic Next button label.
    /// </summary>
    public NodeBuilder<TModel, TResult> Next(Func<TModel, Task<Result<IWizardNode<TResult>?>>> nextSelector, IObservable<bool>? canExecute, IObservable<string> nextLabel)
    {
        this.nextFactory = async m =>
        {
            var result = await nextSelector(m);
            if (result.IsFailure)
            {
                return Result.Failure<WizardResult<TResult>>(result.Error);
            }

            return result.Value != null
                ? Result.Success(WizardResult<TResult>.Continue(result.Value))
                : Result.Failure<WizardResult<TResult>>("No next node");
        };

        if (canExecute != null)
        {
            this.canNext = canExecute;
        }

        this.nextLabel = nextLabel;

        return this;
    }

    /// <summary>
    /// Marks this node as the final step that produces the wizard result.
    /// </summary>
    /// <param name="resultSelector">Function that extracts the result from the model.</param>
    /// <param name="canExecute">Optional observable controlling when the wizard can finish.</param>
    /// <param name="nextLabel">Optional label for the Finish button.</param>
    public NodeBuilder<TModel, TResult> Finish(Func<TModel, TResult> resultSelector, IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        this.nextFactory = m =>
        {
            var result = resultSelector(m);
            return Task.FromResult(Result.Success(WizardResult<TResult>.Complete(result)));
        };

        if (canExecute != null)
        {
            this.canNext = canExecute;
        }

        if (nextLabel != null)
        {
            this.nextLabel = Observable.Return(nextLabel);
        }

        return this;
    }

    /// <summary>
    /// Marks this node as the final step that produces the wizard result with a dynamic Finish button label.
    /// </summary>
    public NodeBuilder<TModel, TResult> Finish(Func<TModel, TResult> resultSelector, IObservable<bool>? canExecute, IObservable<string> nextLabel)
    {
        this.nextFactory = m =>
        {
            var result = resultSelector(m);
            return Task.FromResult(Result.Success(WizardResult<TResult>.Complete(result)));
        };

        if (canExecute != null)
        {
            this.canNext = canExecute;
        }

        this.nextLabel = nextLabel;

        return this;
    }

    /// <summary>
    /// Marks this node as the final step that produces the wizard result asynchronously.
    /// </summary>
    /// <param name="resultSelector">Async function that extracts the result from the model.</param>
    /// <param name="canExecute">Optional observable controlling when the wizard can finish.</param>
    /// <param name="nextLabel">Optional label for the Finish button.</param>
    public NodeBuilder<TModel, TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector, IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        this.nextFactory = async m =>
        {
            var result = await resultSelector(m);
            return result.IsSuccess
                ? Result.Success(WizardResult<TResult>.Complete(result.Value))
                : Result.Failure<WizardResult<TResult>>(result.Error);
        };

        if (canExecute != null)
        {
            this.canNext = canExecute;
        }

        if (nextLabel != null)
        {
            this.nextLabel = Observable.Return(nextLabel);
        }

        return this;
    }

    /// <summary>
    /// Marks this node as the final step that produces the wizard result asynchronously with a dynamic Finish button label.
    /// </summary>
    public NodeBuilder<TModel, TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector, IObservable<bool>? canExecute, IObservable<string> nextLabel)
    {
        this.nextFactory = async m =>
        {
            var result = await resultSelector(m);
            return result.IsSuccess
                ? Result.Success(WizardResult<TResult>.Complete(result.Value))
                : Result.Failure<WizardResult<TResult>>(result.Error);
        };

        if (canExecute != null)
        {
            this.canNext = canExecute;
        }

        this.nextLabel = nextLabel;

        return this;
    }
}