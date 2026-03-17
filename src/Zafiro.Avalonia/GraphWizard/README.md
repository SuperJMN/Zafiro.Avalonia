# GraphWizard API Documentation

## Overview

`GraphWizard` is a graph-based wizard system for Avalonia that enables creating multi-step navigation flows with conditional branching. Unlike traditional linear wizards, GraphWizard allows building complex flows where each step can lead to
different subsequent steps based on business logic.

## Key Concepts

### IWizardNode

Represents an individual node in the wizard graph. Each node encapsulates:

- **Content**: The ViewModel or content to be displayed in this step
- **Title**: An observable that provides the step title (allows dynamic titles)
- **Next**: A command that determines the next step (can return another node or null to finish)

### GraphWizard

The main class that orchestrates the wizard flow:

- Manages navigation between nodes
- Provides commands for Back, Next, and Cancel
- Maintains a navigation stack to allow going back
- Implements `IHaveHeader` and `IHaveFooter` to provide automatic UI

## Builder API

### GraphWizardBuilder

The recommended way to create wizard nodes is through the fluent builder API.

For typed wizards, prefer fixing the result type once:

```csharp
var graph = GraphWizardBuilder.For<string>();
```

#### Main Methods

##### `For<TResult>()`

Creates a typed builder context that avoids repeating `TResult` on every `Define(...)`.

```csharp
var graph = GraphWizardBuilder.For<string>();

var endNode = graph.Define(endVm, "Completed")
    .Finish(vm => "done")
    .Build();
```

---

##### `Define<TModel>(model, title)`

Starts building a node with a model and title.

**Overloads:**

1. **With static title:**

```csharp
Define<TModel>(TModel model, string title)
```

2. **With dynamic title:**

```csharp
Define<TModel>(TModel model, IObservable<string> title)
```

**Parameters:**

- `model`: The ViewModel or content for the step
- `title`: `string` or `IObservable<string>` - The step title

**Returns:** `NodeBuilder<TModel>` to continue configuring the node

**Examples:**

```csharp
// Static title
var nodeBuilder = GraphWizardBuilder.Define(viewModel, "My Step");

// Dynamic title based on ViewModel properties
var nodeBuilder = GraphWizardBuilder.Define(
    viewModel, 
    viewModel.WhenAnyValue(x => x.DynamicTitle)
);
```

For typed wizards:

```csharp
var graph = GraphWizardBuilder.For<string>();
var nodeBuilder = graph.Define(viewModel, "My Step");
```

---

##### `Next(nextSelector, canExecute?)`

Defines the next step in the flow.

**Overloads:**

1. **Synchronous with IWizardNode:**

```csharp
Next(Func<TModel, IWizardNode?> nextSelector, IObservable<bool>? canExecute = null)
```

2. **Asynchronous with Task:**

```csharp
Next(Func<TModel, Task<Result<IWizardNode?>>> nextSelector, IObservable<bool>? canExecute = null)
```

**Parameters:**

- `nextSelector`: Function that receives the current model and returns the next node. Returning `null` finishes the wizard.
- `canExecute` (optional): Observable that controls when the Next command can execute

**Returns:** The same `NodeBuilder<TModel>` for chaining

**Examples:**

```csharp
// Simple conditional navigation
.Next(vm => vm.IsOptionA ? nodeA : nodeB)

// With validation
.Next(vm => vm.Choice == "A" ? nodeA : nodeB,
      canExecute: vm.WhenAnyValue(x => x.Choice).NotNull())

// Asynchronous navigation (e.g., saving data)
.Next(async vm => 
{
    var result = await vm.SaveData();
    return result.IsSuccess ? nextNode : null;
})
```

---

##### `Finish(canExecute?)`

Marks this node as the final step of the wizard.

**Parameters:**

- `canExecute` (optional): Observable that controls when the wizard can be finished

**Returns:** The same `NodeBuilder<TModel>` for chaining

```csharp
var finalNode = GraphWizardBuilder.Define(completionViewModel, "Finished")
    .Finish()
    .Build();

// With validation to finish
.Finish(canExecute: vm.WhenAnyValue(x => x.AllRequiredFieldsCompleted))
```

---

##### `Build()`

Builds and returns the configured `IWizardNode`.

**Returns:** `IWizardNode`

```csharp
var node = GraphWizardBuilder.Define(viewModel, "Step 1")
    .Next(vm => nextNode)
    .Build();
```

---

### GraphWizard (Constructor)

Creates a graph wizard instance.

```csharp
public GraphWizard(IWizardNode initialNode, IObservable<string>? nextTitle = null)
```

**Parameters:**

- `initialNode`: The initial wizard node
- `nextTitle` (optional): Custom title for the "Next" button. Defaults to "Next"

**Properties:**

- `CurrentStep`: The current node in the flow
- `Back`: Command to go back to the previous step
- `Next`: Command to advance to the next step
- `Cancel`: Command to cancel the wizard
- `Finish`: Observable that emits when the wizard finishes
- `NextTitle`: Observable with the Next button title

```csharp
// Static title
var wizard = new GraphWizard(startNode, Observable.Return("Continue"));

// Dynamic title based on ViewModel state
var wizard = new GraphWizard(startNode, 
    viewModel.WhenAnyValue(x => x.Choice)
        .Select(choice => choice switch
        {
            "A" => "Go to A",
            "B" => "Go to B",
            _ => "Next"
        }));
```

---

### Extension Methods

The GraphWizard API provides convenient extension methods to simplify common usage patterns.

#### `Navigate(INavigator)`

Navigates to the wizard and automatically handles back navigation when the wizard finishes.

**Namespace:** `Zafiro.Avalonia.Wizards.Graph.Core`

```csharp
public static async Task Navigate(this GraphWizard wizard, INavigator navigator)
```

**Example:**

```csharp
var wizard = CreateWizard();
await wizard.Navigate(navigator);
// The wizard automatically goes back when finished
```

This is equivalent to:

```csharp
var wizard = CreateWizard();
wizard.Finish.Subscribe(_ => navigator.GoBack());
await navigator.Go(() => wizard);
```

---

#### `ShowInDialog(IDialog, string)`

Shows the wizard in a dialog and automatically closes the dialog when the wizard finishes.

**Namespace:** `Zafiro.Avalonia.Dialogs`

```csharp
public static Task<bool> ShowInDialog(
    this GraphWizard wizard,
    IDialog dialog,
    string title,
    Func<GraphWizard, ICloseable, IEnumerable<IOption>>? optionsFactory = null)
```

**Parameters:**

- `dialog`: The dialog service to use
- `title`: The dialog title
- `optionsFactory` (optional): Factory to create additional dialog options beyond the wizard's own buttons

**Returns:** `Task<bool>` - true if the dialog was closed via an option, false if cancelled

**Examples:**

```csharp
// Simple usage
var wizard = CreateWizard();
await wizard.ShowInDialog(dialog, "Configuration Wizard");

// With additional options
await wizard.ShowInDialog(dialog, "Setup Wizard", (w, closeable) =>
{
    return new[]
    {
        new Option("Help", () => ShowHelp()),
        new Option("Reset", () => w.GoBack())
    };
});
```

---

#### `ShowInDialog(IDialog, IObservable<string>)`

Shows the wizard in a dialog with a dynamic title observable.

```csharp
public static Task<bool> ShowInDialog(
    this GraphWizard wizard,
    IDialog dialog,
    IObservable<string> title,
    Func<GraphWizard, ICloseable, IEnumerable<IOption>>? optionsFactory = null)
```

**Example:**

```csharp
var wizard = CreateWizard();
var titleObservable = wizard.WhenAnyValue(x => x.CurrentStep)
    .Select(step => $"Wizard - {step.Title}");
    
await wizard.ShowInDialog(dialog, titleObservable);
```

---

## Complete Usage Pattern

### Example: Wizard with Branching

```csharp
// 1. Define the end node
var endVm = new CompletionViewModel();
var endNode = GraphWizardBuilder.Define(endVm, "Completed")
    .Finish()
    .Build();

// 2. Define branch B
var stepB_Vm = new PathBViewModel();
var nodeB = GraphWizardBuilder.Define(stepB_Vm, "Path B")
    .Next(_ => endNode)
    .Build();

// 3. Define branch A
var stepA_Vm = new PathAViewModel();
var nodeA = GraphWizardBuilder.Define(stepA_Vm, "Path A")
    .Next(_ => endNode)
    .Build();

// 4. Define initial node with branching logic
var startVm = new ChoiceViewModel();
var startNode = GraphWizardBuilder.Define(startVm, "Select Path")
    .Next(vm => vm.Choice == "A" ? nodeA : nodeB,
          canExecute: startVm.WhenAnyValue(x => x.Choice).NotNull())
    .Build();

// 5. Create the wizard
var wizard = new GraphWizard(startNode);

// 6. Navigate using extension method (automatically handles back navigation)
await wizard.Navigate(navigator);
```

### Example: Wizard in Navigation (Manual Approach)

If you prefer manual control:

```csharp
var wizard = CreateWizard();

// Subscribe to completion
wizard.Finish.Subscribe(_ => navigator.GoBack());

// Navigate to the wizard
await navigator.Go(() => wizard);
```

### Example: Wizard in Dialog (Recommended)

```csharp
var wizard = CreateWizard();

// Simple usage with just a title
await wizard.ShowInDialog(dialog, "My Wizard");

// With dynamic title
var titleObservable = Observable.Return("Configuration Wizard");
await wizard.ShowInDialog(dialog, titleObservable);

// With custom options
await wizard.ShowInDialog(dialog, "My Wizard", (w, closeable) =>
{
    return new[]
    {
        new Option("Help", () => ShowHelp())
    };
});
```

### Example: Wizard in Dialog (Manual Approach)

If you need full control:

```csharp
var wizard = CreateWizard();

await dialog.Show(wizard, Observable.Return("My Wizard"), (w, closeable) =>
{
    // Close the dialog when the wizard finishes
    w.Finish.Subscribe(_ => closeable.Close());
    
    // Don't show additional buttons (the wizard already has its own)
    return Enumerable.Empty<IOption>();
});
```

### Example: Asynchronous Validation

```csharp
var node = GraphWizardBuilder.Define(viewModel, "Enter Data")
    .Next(async vm =>
    {
        // Validate on the server
        var validationResult = await vm.ValidateOnServer();
        if (validationResult.IsFailure)
        {
            // Show error and remain on current step
            vm.ErrorMessage = validationResult.Error;
            return Result.Failure<IWizardNode?>("Validation failed");
        }
        
        // Proceed to next step
        return Result.Success<IWizardNode?>(nextNode);
    },
    canExecute: viewModel.WhenAnyValue(
        x => x.Field1, x => x.Field2,
        (f1, f2) => !string.IsNullOrEmpty(f1) && !string.IsNullOrEmpty(f2)))
    .Build();
```

---

## Advanced Features

### Dynamic "Next" Button Title

The Next button title can change dynamically based on context:

```csharp
var nextTitleObservable = viewModel.WhenAnyValue(x => x.SelectedOption)
    .Select(option => option switch
    {
        "Save" => "Save and Continue",
        "Skip" => "Skip",
        _ => "Next"
    });

var wizard = new GraphWizard(startNode, nextTitleObservable);
```

### Integration with IHaveHeader/IHaveFooter

Step ViewModels can implement `IHaveHeader` and `IHaveFooter` to provide custom content:

```csharp
public class StepViewModel : ReactiveObject, IHaveHeader, IHaveFooter
{
    public IObservable<object> Header => Observable.Return("Custom Header");
    public IObservable<object> Footer => Observable.Return("Footer Note");
}
```

When used inside a `Frame`, the step's headers/footers will be displayed in the Frame, not in the GraphWizardView.

### Back Navigation (Back Stack)

GraphWizard automatically maintains a navigation stack:

- The `Back` command is enabled when there are previous steps
- `Back.CanExecute` updates automatically
- The stack is cleared when the wizard finishes

### Wizard Completion

A node finishes the wizard by returning `null` from its Next function:

```csharp
// Explicit form
.Next(_ => (IWizardNode?)null)

// Convenient form
.Finish()

// With validation
.Finish(canExecute: vm.WhenAnyValue(x => x.IsComplete))
```

When the wizard finishes:

1. The `Finish` observable emits
2. The `Next` command executed successfully but returned `null`
3. The `CurrentStep` remains on the last node

---

## Best Practices

### 1. Build Bottom-Up

Define the end nodes first and work backwards to the initial node:

```csharp
var end = ...Build();
var middle = ...Next(_ => end).Build();
var start = ...Next(_ => middle).Build();
```

### 2. Step Validation

Use `canExecute` to validate each step before allowing advance:

```csharp
.Next(vm => nextNode, 
      canExecute: vm.WhenAnyValue(x => x.IsValid))
```

### 3. Error Handling

For asynchronous operations, return `Result.Failure` to remain on the current step:

```csharp
.Next(async vm =>
{
    var result = await vm.SaveAsync();
    return result.IsSuccess 
        ? Result.Success<IWizardNode?>(nextNode)
        : Result.Failure<IWizardNode?>(result.Error);
})
```

### 4. Descriptive Titles

Use clear titles that describe each step:

```csharp
GraphWizardBuilder.Define(vm, "Select Account")
```

### 5. Subscribe to Finish

Always handle the completion event to clean up or navigate:

```csharp
wizard.Finish.Subscribe(_ => navigator.GoBack());
```

---

## UI Integration

### GraphWizardView

The `GraphWizardView` control provides the default UI:

- Displays the current step content
- Renders header/footer automatically
- Integrates with Zafiro's Frame system

### Default Template

```xml
<view:GraphWizardView DataContext="{Binding MyWizard}" />
```

The control will display:

- Step header (if `IHaveHeader`)
- Current step content
- Wizard footer with Back/Next/Cancel buttons

---

## Type Reference

### Namespaces

- `Zafiro.Avalonia.Wizards.Graph.Core` - Core classes
- `Zafiro.Avalonia.Wizards.Graph.Builder` - Builder API
- `Zafiro.Avalonia.Wizards.Graph.View` - UI controls

### Main Interfaces

- `IWizardNode` - Node contract
- `IHaveHeader` - For ViewModels with header
- `IHaveFooter` - For ViewModels with footer

### Main Classes

- `GraphWizard` - Wizard engine
- `GraphWizardBuilder` - Fluent API for construction
- `NodeBuilder<TModel>` - Typed node builder
- `WizardNode` - Node implementation
- `GraphWizardView` - Avalonia UI control
