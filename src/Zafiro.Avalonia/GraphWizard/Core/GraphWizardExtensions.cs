using CSharpFunctionalExtensions;
using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

public static class GraphWizardExtensions
{
    /// <summary>
    /// Creates an untyped graph wizard with a custom Next button title.
    /// </summary>
    /// <param name="wizard">The wizard instance.</param>
    /// <param name="nextTitle">The title for the Next button.</param>
    /// <returns>A new GraphWizard instance.</returns>
    public static GraphWizard WithNextTitle(this GraphWizard wizard, string nextTitle)
    {
        return new GraphWizard(wizard.CurrentStep!, Observable.Return(nextTitle));
    }

    /// <summary>
    /// Creates an untyped graph wizard with a custom Next button title observable.
    /// </summary>
    /// <param name="wizard">The wizard instance.</param>
    /// <param name="nextTitle">An observable that emits the title for the Next button.</param>
    /// <returns>A new GraphWizard instance.</returns>
    public static GraphWizard WithNextTitle(this GraphWizard wizard, IObservable<string> nextTitle)
    {
        return new GraphWizard(wizard.CurrentStep!, nextTitle);
    }

    /// <summary>
    /// Navigates to an untyped wizard and automatically goes back when it finishes.
    /// </summary>
    /// <param name="wizard">The wizard to navigate to.</param>
    /// <param name="navigator">The navigator to use for navigation.</param>
    /// <returns>A task that completes when the wizard is shown.</returns>
    /// <example>
    /// <code>
    /// var wizard = CreateWizard();
    /// await wizard.Navigate(navigator);
    /// </code>
    /// </example>
    public static async Task Navigate(this GraphWizard wizard, INavigator navigator)
    {
        wizard.Finish.Subscribe(_ => navigator.GoBack());
        await navigator.Go(() => wizard);
    }

    /// <summary>
    /// Navigates to a typed wizard and returns its result when finished.
    /// Automatically goes back when the wizard finishes or is cancelled.
    /// </summary>
    /// <typeparam name="TResult">The type of the wizard result.</typeparam>
    /// <param name="wizard">The wizard to navigate to.</param>
    /// <param name="navigator">The navigator to use for navigation.</param>
    /// <returns>A task that returns Maybe.Some(result) if completed, or Maybe.None if cancelled.</returns>
    public static async Task<Maybe<TResult>> Navigate<TResult>(this GraphWizard<TResult> wizard, INavigator navigator)
    {
        var finished = wizard.Finished.Select(Maybe.From).FirstOrDefaultAsync();
        await navigator.Go(() => wizard);
        var result = await finished;
        await navigator.GoBack();
        return result;
    }
}