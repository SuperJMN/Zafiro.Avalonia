using System.Reactive.Linq;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Dialogs;

/// <summary>
/// Extension methods for showing <see cref="GraphWizard"/> in dialogs.
/// </summary>
public static class GraphWizardDialogExtensions
{
    /// <summary>
    /// Shows the wizard in a dialog and automatically closes the dialog when the wizard finishes.
    /// </summary>
    /// <param name="wizard">The wizard to show in the dialog.</param>
    /// <param name="dialog">The dialog service to use.</param>
    /// <param name="title">The title for the dialog.</param>
    /// <param name="optionsFactory">
    /// Optional factory to create additional dialog options. 
    /// The wizard's finish handler is automatically set up to close the dialog.
    /// </param>
    /// <returns>A task that returns true if the dialog was closed via an option, false if cancelled.</returns>
    /// <example>
    /// <code>
    /// // Simple usage with just a title
    /// var wizard = CreateWizard();
    /// await wizard.ShowInDialog(dialog, "Graph Wizard");
    /// 
    /// // With custom options
    /// await wizard.ShowInDialog(dialog, "Graph Wizard", (w, closeable) =>
    /// {
    ///     return new[]
    ///     {
    ///         new Option("Help", () => ShowHelp())
    ///     };
    /// });
    /// </code>
    /// </example>
    public static Task<bool> ShowInDialog(
        this GraphWizard wizard,
        IDialog dialog,
        string title,
        Func<GraphWizard, ICloseable, IEnumerable<IOption>>? optionsFactory = null)
    {
        return ShowInDialog(wizard, dialog, Observable.Return(title), optionsFactory);
    }

    /// <summary>
    /// Shows the wizard in a dialog and automatically closes the dialog when the wizard finishes.
    /// </summary>
    /// <param name="wizard">The wizard to show in the dialog.</param>
    /// <param name="dialog">The dialog service to use.</param>
    /// <param name="title">An observable that provides the dialog title.</param>
    /// <param name="optionsFactory">
    /// Optional factory to create additional dialog options. 
    /// The wizard's finish handler is automatically set up to close the dialog.
    /// </param>
    /// <returns>A task that returns true if the dialog was closed via an option, false if cancelled.</returns>
    /// <example>
    /// <code>
    /// var wizard = CreateWizard();
    /// var titleObservable = Observable.Return("Graph Wizard");
    /// await wizard.ShowInDialog(dialog, titleObservable);
    /// </code>
    /// </example>
    public static Task<bool> ShowInDialog(
        this GraphWizard wizard,
        IDialog dialog,
        IObservable<string> title,
        Func<GraphWizard, ICloseable, IEnumerable<IOption>>? optionsFactory = null)
    {
        return dialog.Show(wizard, title, (w, closeable) =>
        {
            // Set up automatic dialog close when wizard finishes
            w.Finish.Subscribe(_ => closeable.Close());

            // If user provided additional options, include them
            return optionsFactory?.Invoke(w, closeable) ?? Enumerable.Empty<IOption>();
        });
    }

    /// <summary>
    /// Shows the wizard in a dialog and returns its result when finished.
    /// Automatically closes the dialog when the wizard finishes or is cancelled.
    /// </summary>
    /// <typeparam name="TResult">The type of the wizard result.</typeparam>
    /// <param name="wizard">The wizard to show in the dialog.</param>
    /// <param name="dialog">The dialog service to use.</param>
    /// <param name="title">The title for the dialog.</param>
    /// <param name="optionsFactory">
    /// Optional factory to create additional dialog options. 
    /// The wizard's finish handler is automatically set up to close the dialog.
    /// </param>
    /// <returns>A task that returns Maybe.Some(result) if completed, or Maybe.None if cancelled.</returns>
    public static Task<Maybe<TResult>> ShowInDialog<TResult>(
        this GraphWizard<TResult> wizard,
        IDialog dialog,
        string title,
        Func<GraphWizard<TResult>, ICloseable, IEnumerable<IOption>>? optionsFactory = null)
    {
        return ShowInDialog(wizard, dialog, Observable.Return(title), optionsFactory);
    }

    /// <summary>
    /// Shows the wizard in a dialog and returns its result when finished.
    /// Automatically closes the dialog when the wizard finishes or is cancelled.
    /// </summary>
    /// <typeparam name="TResult">The type of the wizard result.</typeparam>
    /// <param name="wizard">The wizard to show in the dialog.</param>
    /// <param name="dialog">The dialog service to use.</param>
    /// <param name="title">An observable that provides the dialog title.</param>
    /// <param name="optionsFactory">
    /// Optional factory to create additional dialog options. 
    /// The wizard's finish handler is automatically set up to close the dialog.
    /// </param>
    /// <returns>A task that returns Maybe.Some(result) if completed, or Maybe.None if cancelled.</returns>
    public static async Task<Maybe<TResult>> ShowInDialog<TResult>(
        this GraphWizard<TResult> wizard,
        IDialog dialog,
        IObservable<string> title,
        Func<GraphWizard<TResult>, ICloseable, IEnumerable<IOption>>? optionsFactory = null)
    {
        var result = Maybe<TResult>.None;
        using var _ = wizard.Finished.Subscribe(r => result = Maybe.From(r));

        await dialog.Show(wizard, title, (w, closeable) =>
        {
            // Set up automatic dialog close when wizard finishes
            w.Finish.Subscribe(_ => closeable.Close());

            // If user provided additional options, include them
            return optionsFactory?.Invoke(w, closeable) ?? Enumerable.Empty<IOption>();
        });

        return result;
    }
}