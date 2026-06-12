namespace Zafiro.Avalonia.Wizards.Graph.Core;

/// <summary>
/// Definition of a step in a wizard graph (a graph vertex).
/// <para>
/// A step is a lightweight, reusable description of how to build a node. Every time the
/// wizard enters a step it <see cref="CreateNode"/> a fresh <see cref="IWizardNode{TResult}"/>
/// with a newly created content/view-model. This means step view models receive their state
/// by constructor, are used while the step is current, and are discarded when the step is left.
/// </para>
/// <para>
/// As a consequence, step view models hold no navigation awareness and no long-lived
/// invariants: state is hoisted out of them (e.g. into a shared session passed to the factory),
/// and the wizard never reuses a stale instance. This is what lets <c>Reset()</c>-style cleanup
/// and per-node lifecycle hooks (<c>OnNavigatedTo</c>/<c>OnEntered</c>) disappear entirely.
/// </para>
/// </summary>
/// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
public interface IWizardStep<TResult>
{
    /// <summary>
    /// Creates a fresh node for this step, including a newly created content/view-model.
    /// Called once per entry into the step (forward navigation and back navigation alike).
    /// </summary>
    IWizardNode<TResult> CreateNode();
}
