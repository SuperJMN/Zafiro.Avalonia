# GraphWizard

`GraphWizard` is Zafiro's graph-based wizard API for Avalonia.

Use it when a flow is not strictly linear and the next step depends on user choices or runtime logic.

## Recommended API

For public app code, start from `GraphWizard.For<TResult>()` and create steps with `Step(...)`.

```csharp
var flow = GraphWizard.For<string>();

var end = flow.Step(new SummaryViewModel(), "Done")
    .Finish(_ => "completed", nextLabel: "Finish")
    .Build();

var optionA = flow.Step(new OptionAViewModel(), "Option A")
    .Next(_ => end)
    .Build();

var optionB = flow.Step(new OptionBViewModel(), "Option B")
    .Next(_ => end)
    .Build();

var start = flow.Step(new StartViewModel(), "Start")
    .Next(vm => vm.UseA ? optionA : optionB, canExecute: vm.WhenAnyValue(x => x.SelectionMade))
    .Build();

var wizard = new GraphWizard<string>(start);
```

## Core Ideas

- `GraphWizard.For<TResult>()`
  Fixes the result type once for the whole flow.
- `Step(...)`
  Starts a typed wizard step.
- `Next(...)`
  Moves to another node.
- `Finish(...)`
  Completes the wizard with a typed result.
- `Build()`
  Produces the `IWizardNode<TResult>` consumed by `GraphWizard<TResult>`.

## Linear Flows

If the flow is mostly linear, `StartWith(...)` is available:

```csharp
var start = GraphWizard.For<string>()
    .StartWith(new AccountViewModel(), "Account")
    .Step(new DetailsViewModel(), "Details")
    .Finish(vm => vm.Result);
```

## Display

You can navigate to a wizard:

```csharp
await wizard.Navigate(navigator);
```

Or show it in a dialog:

```csharp
var result = await wizard.ShowInDialog(dialog, "Create project");
```

## Notes

- Use `GraphWizardBuilder.Step(...)` only for untyped wizards.
- Prefer `GraphWizard.For<TResult>()` for public app code.
- `Next(...)` can use `canExecute` and dynamic labels through `IObservable<T>`.
