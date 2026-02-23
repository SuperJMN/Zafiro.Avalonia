using CSharpFunctionalExtensions;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// Default implementation of <see cref="IWizardNode{TResult}"/>.
/// Typically created via <see cref="Builder.GraphWizardBuilderGeneric"/> rather than directly.
/// </summary>
/// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
public class WizardNodeGeneric<TResult> : IWizardNode<TResult>
{
    private readonly Func<Task<Result<WizardResult<TResult>>>> nextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardNodeGeneric{TResult}"/> class.
    /// </summary>
    /// <param name="content">The content (typically a ViewModel) to display for this step.</param>
    /// <param name="title">An observable that emits the title for this step.</param>
    /// <param name="nextFactory">A factory function that determines the next node or result when executed.</param>
    /// <param name="canNext">An observable that controls when the Next command can execute.</param>
    /// <param name="nextLabel">An observable that emits the label for the Next button. Defaults to "Next".</param>
    public WizardNodeGeneric(object content, IObservable<string> title, Func<Task<Result<WizardResult<TResult>>>> nextFactory, IObservable<bool> canNext, IObservable<string>? nextLabel = null)
    {
        Content = content;
        Title = title;
        NextLabel = nextLabel ?? Observable.Return("Next");
        this.nextFactory = nextFactory;

        var logic = ReactiveCommand.CreateFromTask(nextFactory, canNext);
        Next = logic.Enhance("Next");
    }

    /// <inheritdoc />
    public IObservable<string> Title { get; }

    /// <inheritdoc />
    public object Content { get; }

    /// <inheritdoc />
    public IObservable<string> NextLabel { get; }

    /// <inheritdoc />
    public IEnhancedCommand<Result<WizardResult<TResult>>> Next { get; }
}