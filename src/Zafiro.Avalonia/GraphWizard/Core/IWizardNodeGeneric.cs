using CSharpFunctionalExtensions;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// Represents a node in a wizard graph that produces a typed result.
/// Each node contains content to display, a title, and logic to determine the next step or completion.
/// </summary>
/// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
public interface IWizardNode<TResult> : IBaseWizardNode
{
    /// <summary>
    /// Gets the command that executes when advancing to the next step.
    /// Returns either the next <see cref="IWizardNode{TResult}"/> to navigate to, or a result to complete the wizard.
    /// </summary>
    public IEnhancedCommand<Result<WizardResult<TResult>>> Next { get; }
}