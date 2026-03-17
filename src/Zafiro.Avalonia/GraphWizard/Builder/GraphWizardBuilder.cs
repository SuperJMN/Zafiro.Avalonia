using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Wizards.Graph.Core;

namespace Zafiro.Avalonia.Wizards.Graph.Builder;

/// <summary>
/// Provides a fluent API for building wizard nodes and graph flows.
/// Use <see cref="Define{TModel}(TModel, string)"/> or <see cref="Define{TModel}(TModel, IObservable{string})"/> 
/// to start building a wizard node.
/// </summary>
public class GraphWizardBuilder
{
    /// <summary>
    /// Creates a typed builder context where the wizard result type is specified once and reused for all nodes.
    /// </summary>
    /// <typeparam name="TResult">The type of result the wizard will produce.</typeparam>
    /// <returns>A typed builder context for constructing result-producing graph wizards.</returns>
    public static TypedGraphWizardBuilder<TResult> For<TResult>()
    {
        return new TypedGraphWizardBuilder<TResult>();
    }

    /// <summary>
    /// Starts building a wizard node with a static title.
    /// </summary>
    /// <typeparam name="TModel">The type of the view model or content for this step.</typeparam>
    /// <param name="model">The view model or content to display in this wizard step.</param>
    /// <param name="title">The static title for this step.</param>
    /// <returns>A <see cref="NodeBuilder{TModel}"/> to configure the node further.</returns>
    /// <example>
    /// <code>
    /// var node = GraphWizardBuilder.Define(viewModel, "Step 1")
    ///     .Next(vm => nextNode)
    ///     .Build();
    /// </code>
    /// </example>
    public static NodeBuilder<TModel> Define<TModel>(TModel model, string title)
    {
        return new NodeBuilder<TModel>(model, title);
    }

    /// <summary>
    /// Starts building a wizard node with a dynamic title.
    /// </summary>
    /// <typeparam name="TModel">The type of the view model or content for this step.</typeparam>
    /// <param name="model">The view model or content to display in this wizard step.</param>
    /// <param name="title">An observable that emits the title for this step. Allows the title to change dynamically.</param>
    /// <returns>A <see cref="NodeBuilder{TModel}"/> to configure the node further.</returns>
    /// <example>
    /// <code>
    /// var node = GraphWizardBuilder.Define(viewModel, viewModel.WhenAnyValue(x => x.DynamicTitle))
    ///     .Next(vm => nextNode)
    ///     .Build();
    /// </code>
    /// </example>
    public static NodeBuilder<TModel> Define<TModel>(TModel model, IObservable<string> title)
    {
        return new NodeBuilder<TModel>(model, title);
    }
}

/// <summary>
/// Interface for building wizard nodes.
/// </summary>
/// <typeparam name="TModel">The type of the model for the node being built.</typeparam>
public interface INodeBuilder<out TModel>
{
    /// <summary>
    /// Builds and returns the configured <see cref="IWizardNode"/>.
    /// </summary>
    /// <returns>The constructed wizard node.</returns>
    IWizardNode Build();
}

/// <summary>
/// Fluent builder for configuring wizard nodes.
/// Provides methods to define navigation logic and validation.
/// </summary>
/// <typeparam name="TModel">The type of the view model or content for this node.</typeparam>
public class NodeBuilder<TModel> : INodeBuilder<TModel>
{
    private readonly TModel model;
    private readonly IObservable<string> title;
    private IObservable<bool> canNext = Observable.Return(true);

    private Func<TModel, Task<Result<IWizardNode?>>> nextFactory = _ =>
        Task.FromResult(Result.Success<IWizardNode?>(null));

    private IObservable<string>? nextLabel;

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeBuilder{TModel}"/> class with a static title.
    /// </summary>
    /// <param name="model">The view model or content for this step.</param>
    /// <param name="title">The static title for this step.</param>
    public NodeBuilder(TModel model, string title)
    {
        this.model = model;
        this.title = Observable.Return(title);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeBuilder{TModel}"/> class with a dynamic title.
    /// </summary>
    /// <param name="model">The view model or content for this step.</param>
    /// <param name="title">An observable that emits the title for this step.</param>
    public NodeBuilder(TModel model, IObservable<string> title)
    {
        this.model = model;
        this.title = title;
    }

    /// <summary>
    /// Builds and returns the configured <see cref="IWizardNode"/>.
    /// Must be called after configuring navigation with <see cref="Next(Func{TModel, IWizardNode?}, IObservable{bool}?)"/> or <see cref="Finish"/>.
    /// </summary>
    /// <returns>The constructed wizard node.</returns>
    public IWizardNode Build()
    {
        return new WizardNode(model!, title, () => nextFactory(model), canNext, nextLabel);
    }

    /// <summary>
    /// Defines asynchronous navigation logic for this node.
    /// Use this overload when next step determination requires async operations (e.g., validation, API calls).
    /// </summary>
    /// <param name="nextSelector">
    /// A function that receives the current model and returns a task with the next node.
    /// Return null to finish the wizard. Return a failure Result to stay on the current step.
    /// </param>
    /// <param name="canExecute">
    /// Optional observable that controls when the Next command can execute (e.g., for validation).
    /// Defaults to always enabled.
    /// </param>
    /// <param name="nextLabel">Optional label for the Next button.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// .Next(async vm =>
    /// {
    ///     var result = await vm.ValidateAsync();
    ///     return result.IsSuccess ? Result.Success(nextNode) : Result.Failure("Validation failed");
    /// }, canExecute: vm.WhenAnyValue(x => x.IsValid))
    /// </code>
    /// </example>
    public NodeBuilder<TModel> Next(Func<TModel, Task<Result<IWizardNode?>>> nextSelector,
        IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        this.nextFactory = nextSelector;
        if (canExecute != null)
        {
            this.canNext = canExecute;
        }

        if (nextLabel != null)
        {
            this.nextLabel = Observable.Return(nextLabel);
        }

        return this;
    }

    /// <summary>
    /// Defines asynchronous navigation logic for this node with a dynamic Next button label.
    /// </summary>
    public NodeBuilder<TModel> Next(Func<TModel, Task<Result<IWizardNode?>>> nextSelector,
        IObservable<bool>? canExecute, IObservable<string> nextLabel)
    {
        this.nextFactory = nextSelector;
        if (canExecute != null)
        {
            this.canNext = canExecute;
        }

        this.nextLabel = nextLabel;

        return this;
    }

    /// <summary>
    /// Defines synchronous navigation logic for this node.
    /// Use this overload when next step determination is immediate (no async operations needed).
    /// </summary>
    /// <param name="nextSelector">
    /// A function that receives the current model and returns the next node.
    /// Return null to finish the wizard.
    /// </param>
    /// <param name="canExecute">
    /// Optional observable that controls when the Next command can execute (e.g., for validation).
    /// Defaults to always enabled.
    /// </param>
    /// <param name="nextLabel">Optional label for the Next button.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// .Next(vm => vm.Choice == "A" ? nodeA : nodeB,
    ///       canExecute: vm.WhenAnyValue(x => x.Choice).NotNull())
    /// </code>
    /// </example>
    public NodeBuilder<TModel> Next(Func<TModel, IWizardNode?> nextSelector, IObservable<bool>? canExecute = null,
        string? nextLabel = null)
    {
        return Next(m => Task.FromResult(Result.Success(nextSelector(m))), canExecute, nextLabel);
    }

    /// <summary>
    /// Defines synchronous navigation logic for this node with a dynamic Next button label.
    /// </summary>
    public NodeBuilder<TModel> Next(Func<TModel, IWizardNode?> nextSelector, IObservable<bool>? canExecute,
        IObservable<string> nextLabel)
    {
        return Next(m => Task.FromResult(Result.Success(nextSelector(m))), canExecute, nextLabel);
    }

    /// <summary>
    /// Marks this node as the final step in the wizard.
    /// When this step's Next command executes, the wizard will finish.
    /// </summary>
    /// <param name="canExecute">
    /// Optional observable that controls when the wizard can be finished (e.g., final validation).
    /// Defaults to always enabled.
    /// </param>
    /// <param name="nextLabel">Optional label for the Finish button (e.g. "Finish", "Create").</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// var finalNode = GraphWizardBuilder.Define(completionViewModel, "Finished")
    ///     .Finish(canExecute: vm.WhenAnyValue(x => x.AllCompleted))
    ///     .Build();
    /// </code>
    /// </example>
    public NodeBuilder<TModel> Finish(IObservable<bool>? canExecute = null, string? nextLabel = null)
    {
        return Next(_ => Task.FromResult(Result.Success<IWizardNode?>(null)), canExecute, nextLabel);
    }

    /// <summary>
    /// Marks this node as the final step in the wizard with a dynamic Finish button label.
    /// </summary>
    public NodeBuilder<TModel> Finish(IObservable<bool>? canExecute, IObservable<string> nextLabel)
    {
        return Next(_ => Task.FromResult(Result.Success<IWizardNode?>(null)), canExecute, nextLabel);
    }
}