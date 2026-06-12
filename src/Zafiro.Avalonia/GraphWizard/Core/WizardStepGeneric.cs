using CSharpFunctionalExtensions;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// Default <see cref="IWizardStep{TResult}"/> implementation.
/// Builds a fresh <see cref="WizardNodeGeneric{TResult}"/> on every entry by invoking the
/// supplied model factory and binding the title, guard, label and next logic to that new instance.
/// Created by the typed builder (<see cref="GraphWizard.For{TResult}"/>); rarely constructed directly.
/// </summary>
/// <typeparam name="TModel">The type of content (view model) shown in the step.</typeparam>
/// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
public sealed class WizardStepGeneric<TModel, TResult> : IWizardStep<TResult>
{
    private readonly Func<TModel> modelFactory;
    private readonly Func<TModel, IObservable<string>> title;
    private readonly Func<TModel, Func<Task<Result<WizardResult<TResult>>>>> nextFactory;
    private readonly Func<TModel, IObservable<bool>> canNext;
    private readonly Func<TModel, IObservable<string>>? nextLabel;

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardStepGeneric{TModel, TResult}"/> class.
    /// </summary>
    /// <param name="modelFactory">Creates a fresh content/view-model for each entry.</param>
    /// <param name="title">Resolves the step title for a given content instance.</param>
    /// <param name="nextFactory">Resolves the next/finish logic for a given content instance.</param>
    /// <param name="canNext">Resolves the Next-enabled observable for a given content instance.</param>
    /// <param name="nextLabel">Optionally resolves the Next button label for a given content instance.</param>
    public WizardStepGeneric(
        Func<TModel> modelFactory,
        Func<TModel, IObservable<string>> title,
        Func<TModel, Func<Task<Result<WizardResult<TResult>>>>> nextFactory,
        Func<TModel, IObservable<bool>> canNext,
        Func<TModel, IObservable<string>>? nextLabel = null)
    {
        this.modelFactory = modelFactory;
        this.title = title;
        this.nextFactory = nextFactory;
        this.canNext = canNext;
        this.nextLabel = nextLabel;
    }

    /// <inheritdoc />
    public IWizardNode<TResult> CreateNode()
    {
        var model = modelFactory();
        return new WizardNodeGeneric<TResult>(
            model!,
            title(model),
            nextFactory(model),
            canNext(model),
            nextLabel?.Invoke(model));
    }
}
