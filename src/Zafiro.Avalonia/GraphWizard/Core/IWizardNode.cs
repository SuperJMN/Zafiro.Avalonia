using CSharpFunctionalExtensions;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// Represents a node in the wizard graph. Each node contains content to display,
/// a title, and logic to determine the next step in the workflow.
/// </summary>
public interface IWizardNode : IBaseWizardNode
{
    /// <summary>
    /// Gets the command that executes when advancing to the next step.
    /// Returns the next <see cref="IWizardNode"/> to navigate to, or null to finish the wizard.
    /// </summary>
    public IEnhancedCommand<Result<IWizardNode?>> Next { get; }
}