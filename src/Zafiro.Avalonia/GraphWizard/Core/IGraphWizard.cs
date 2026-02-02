using System.Reactive;
using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// Common interface for graph wizards, both generic and non-generic.
/// </summary>
public interface IGraphWizard : IHaveHeader, IHaveFooter, IReactiveObject
{
    /// <summary>
    /// Gets the command to navigate back to the previous step.
    /// </summary>
    ReactiveCommand<Unit, Unit> Back { get; }

    /// <summary>
    /// Gets the command to advance to the next step.
    /// </summary>
    ReactiveCommand<Unit, Unit> Next { get; }

    /// <summary>
    /// Gets the command to cancel the wizard.
    /// </summary>
    ReactiveCommand<Unit, Unit> Cancel { get; }

    /// <summary>
    /// Gets an observable that emits the current title for the Next button.
    /// </summary>
    IObservable<string> NextTitle { get; }

    /// <summary>
    /// Gets the current wizard step being displayed.
    /// </summary>
    IBaseWizardNode? CurrentStep { get; }
}