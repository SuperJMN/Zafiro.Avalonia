using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Wizards.Graph.Builder;

/// <summary>
/// Internal fluent API for building typed wizard steps.
/// </summary>
internal static class TypedWizardStepBuilder
{
    /// <summary>
    /// Starts building a wizard step with a static title that will produce a typed result.
    /// </summary>
    /// <typeparam name="TModel">The type of the view model or content for this step.</typeparam>
    /// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
    /// <param name="modelFactory">Creates a fresh content/view-model each time the step is entered.</param>
    /// <param name="title">The static title for this step.</param>
    public static TypedWizardStepBuilderCore<TModel, TResult> Step<TModel, TResult>(Func<TModel> modelFactory, string title)
    {
        return new TypedWizardStepBuilderCore<TModel, TResult>(modelFactory, _ => Observable.Return(title));
    }

    /// <summary>
    /// Starts building a wizard step with a content-independent dynamic title.
    /// </summary>
    public static TypedWizardStepBuilderCore<TModel, TResult> Step<TModel, TResult>(Func<TModel> modelFactory, IObservable<string> title)
    {
        return new TypedWizardStepBuilderCore<TModel, TResult>(modelFactory, _ => title);
    }

    /// <summary>
    /// Starts building a wizard step with a title derived from the freshly created content.
    /// </summary>
    public static TypedWizardStepBuilderCore<TModel, TResult> Step<TModel, TResult>(Func<TModel> modelFactory, Func<TModel, IObservable<string>> title)
    {
        return new TypedWizardStepBuilderCore<TModel, TResult>(modelFactory, title);
    }
}

/// <summary>
/// Fluent builder for configuring wizard steps with typed results.
/// <para>
/// Steps are defined by a content factory (<c>() =&gt; new TModel(...)</c>): a fresh content/view-model
/// is created on every entry, and the guard, label and next logic are bound to that new instance.
/// </para>
/// </summary>
/// <typeparam name="TModel">The type of the view model or content for this step.</typeparam>
/// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
internal class TypedWizardStepBuilderCore<TModel, TResult>
{
    private readonly Func<TModel> modelFactory;
    private readonly Func<TModel, IObservable<string>> title;

    private Func<TModel, IObservable<bool>> canNext = _ => Observable.Return(true);

    private Func<TModel, Task<Result<WizardResult<TResult>>>> nextFactory = _ =>
        Task.FromResult(Result.Failure<WizardResult<TResult>>("Next not configured"));

    private Func<TModel, IObservable<string>>? nextLabel;

    public TypedWizardStepBuilderCore(Func<TModel> modelFactory, Func<TModel, IObservable<string>> title)
    {
        this.modelFactory = modelFactory;
        this.title = title;
    }

    /// <summary>
    /// Builds and returns the configured typed wizard step definition.
    /// </summary>
    public IWizardStep<TResult> Build()
    {
        return new WizardStepGeneric<TModel, TResult>(
            modelFactory,
            title,
            model => () => nextFactory(model),
            canNext,
            nextLabel);
    }

    /// <summary>
    /// Defines navigation to the next step.
    /// </summary>
    /// <param name="nextSelector">Function that returns the next step definition (or <see langword="null"/> to fail).</param>
    /// <param name="canExecute">Optional factory of an observable controlling when Next can execute, bound to the fresh content.</param>
    /// <param name="nextLabel">Optional label for the Next button.</param>
    public TypedWizardStepBuilderCore<TModel, TResult> Next(Func<TModel, IWizardStep<TResult>?> nextSelector,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
    {
        SetNext(nextSelector);
        SetCanExecute(canExecute);
        SetLabel(nextLabel);
        return this;
    }

    /// <summary>
    /// Defines navigation to the next step with a dynamic Next button label.
    /// </summary>
    public TypedWizardStepBuilderCore<TModel, TResult> Next(Func<TModel, IWizardStep<TResult>?> nextSelector,
        Func<TModel, IObservable<bool>>? canExecute, Func<TModel, IObservable<string>> nextLabel)
    {
        SetNext(nextSelector);
        SetCanExecute(canExecute);
        this.nextLabel = nextLabel;
        return this;
    }

    /// <summary>
    /// Defines asynchronous navigation to the next step.
    /// </summary>
    public TypedWizardStepBuilderCore<TModel, TResult> Next(
        Func<TModel, Task<Result<IWizardStep<TResult>?>>> nextSelector, Func<TModel, IObservable<bool>>? canExecute = null,
        string? nextLabel = null)
    {
        SetNext(nextSelector);
        SetCanExecute(canExecute);
        SetLabel(nextLabel);
        return this;
    }

    /// <summary>
    /// Defines asynchronous navigation to the next step with a dynamic Next button label.
    /// </summary>
    public TypedWizardStepBuilderCore<TModel, TResult> Next(
        Func<TModel, Task<Result<IWizardStep<TResult>?>>> nextSelector, Func<TModel, IObservable<bool>>? canExecute,
        Func<TModel, IObservable<string>> nextLabel)
    {
        SetNext(nextSelector);
        SetCanExecute(canExecute);
        this.nextLabel = nextLabel;
        return this;
    }

    /// <summary>
    /// Marks this step as the final step that produces the wizard result.
    /// </summary>
    public TypedWizardStepBuilderCore<TModel, TResult> Finish(Func<TModel, TResult> resultSelector,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
    {
        this.nextFactory = m => Task.FromResult(Result.Success(WizardResult<TResult>.Complete(resultSelector(m))));
        SetCanExecute(canExecute);
        SetLabel(nextLabel);
        return this;
    }

    /// <summary>
    /// Marks this step as the final step that produces the wizard result with a dynamic Finish button label.
    /// </summary>
    public TypedWizardStepBuilderCore<TModel, TResult> Finish(Func<TModel, TResult> resultSelector,
        Func<TModel, IObservable<bool>>? canExecute, Func<TModel, IObservable<string>> nextLabel)
    {
        this.nextFactory = m => Task.FromResult(Result.Success(WizardResult<TResult>.Complete(resultSelector(m))));
        SetCanExecute(canExecute);
        this.nextLabel = nextLabel;
        return this;
    }

    /// <summary>
    /// Marks this step as the final step that produces the wizard result asynchronously.
    /// </summary>
    public TypedWizardStepBuilderCore<TModel, TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector,
        Func<TModel, IObservable<bool>>? canExecute = null, string? nextLabel = null)
    {
        SetAsyncFinish(resultSelector);
        SetCanExecute(canExecute);
        SetLabel(nextLabel);
        return this;
    }

    /// <summary>
    /// Marks this step as the final step that produces the wizard result asynchronously with a dynamic Finish button label.
    /// </summary>
    public TypedWizardStepBuilderCore<TModel, TResult> Finish(Func<TModel, Task<Result<TResult>>> resultSelector,
        Func<TModel, IObservable<bool>>? canExecute, Func<TModel, IObservable<string>> nextLabel)
    {
        SetAsyncFinish(resultSelector);
        SetCanExecute(canExecute);
        this.nextLabel = nextLabel;
        return this;
    }

    private void SetNext(Func<TModel, IWizardStep<TResult>?> nextSelector)
    {
        this.nextFactory = m =>
        {
            var next = nextSelector(m);
            return Task.FromResult(next != null
                ? Result.Success(WizardResult<TResult>.Continue(next))
                : Result.Failure<WizardResult<TResult>>("No next step"));
        };
    }

    private void SetNext(Func<TModel, Task<Result<IWizardStep<TResult>?>>> nextSelector)
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
                : Result.Failure<WizardResult<TResult>>("No next step");
        };
    }

    private void SetAsyncFinish(Func<TModel, Task<Result<TResult>>> resultSelector)
    {
        this.nextFactory = async m =>
        {
            var result = await resultSelector(m);
            return result.IsSuccess
                ? Result.Success(WizardResult<TResult>.Complete(result.Value))
                : Result.Failure<WizardResult<TResult>>(result.Error);
        };
    }

    private void SetCanExecute(Func<TModel, IObservable<bool>>? canExecute)
    {
        if (canExecute != null)
        {
            this.canNext = canExecute;
        }
    }

    private void SetLabel(string? label)
    {
        if (label != null)
        {
            this.nextLabel = _ => Observable.Return(label);
        }
    }
}
