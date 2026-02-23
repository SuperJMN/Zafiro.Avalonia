namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// Base interface for all wizard nodes, providing access to display title and content.
/// </summary>
public interface IBaseWizardNode
{
    /// <summary>
    /// Gets an observable that emits the title of this wizard step.
    /// </summary>
    IObservable<string> Title { get; }

    /// <summary>
    /// Gets the content (typically a ViewModel) to display for this wizard step.
    /// </summary>
    object Content { get; }

    /// <summary>
    /// Gets an observable that emits the label for the Next button for this step.
    /// </summary>
    IObservable<string> NextLabel { get; }
}