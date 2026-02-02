using CSharpFunctionalExtensions;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// Default implementation of <see cref="IWizardNode"/>.
/// Typically created via <see cref="Builder.GraphWizardBuilder"/> rather than directly.
/// </summary>
public class WizardNode : IWizardNode
{
    private readonly Func<Task<Result<IWizardNode?>>> nextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardNode"/> class.
    /// </summary>
    /// <param name="content">The content (typically a ViewModel) to display for this step.</param>
    /// <param name="title">An observable that emits the title for this step.</param>
    /// <param name="nextFactory">A factory function that determines the next node when executed.</param>
    /// <param name="canNext">An observable that controls when the Next command can execute.</param>
    /// <param name="nextLabel">An observable that emits the label for the Next button. Defaults to "Next".</param>
    public WizardNode(object content, IObservable<string> title, Func<Task<Result<IWizardNode?>>> nextFactory, IObservable<bool> canNext, IObservable<string>? nextLabel = null)
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
    public IEnhancedCommand<Result<IWizardNode?>> Next { get; }
}