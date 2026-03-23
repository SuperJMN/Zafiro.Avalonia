# Zafiro.Avalonia — Anti-Patterns

> What NOT to do when generating code for Zafiro.Avalonia projects. Every rule is backed by observed evidence. Items marked `[HYPOTHESIS]` are inferred.

---

## ❌ 1. Do Not Use CommunityToolkit.Mvvm

| Wrong | Right |
|---|---|
| `ObservableObject` | `ReactiveObject` |
| `[ObservableProperty]` | `[Reactive]` (ReactiveUI.SourceGenerators) |
| `[RelayCommand]` | `ReactiveCommand.Create()`/`CreateFromTask()` |
| `OnPropertyChanged()` | `WhenAnyValue()` |
| `partial void OnNameChanged()` | `this.WhenAnyValue(x => x.Name).Subscribe()` |

**Why**: Zero CommunityToolkit references in the solution. The entire reactive pipeline (validation, commands, DynamicData, Maybe/Result integration) depends on ReactiveUI.

**Evidence**: `grep -r "CommunityToolkit" . --include="*.cs" --include="*.csproj"` → empty.

---

## ❌ 2. Do Not Put Logic in Code-Behind

```csharp
// ❌ WRONG
public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        MyButton.Click += (s, e) => { /* logic */ };   // NO
    }
}

// ✅ RIGHT
public partial class HomeView : UserControl
{
    public HomeView() => InitializeComponent();
}
```

All behavior belongs in the ViewModel, in Behaviors, or in Converters. Views are AXAML-only shells.

**Evidence**: Every `.axaml.cs` in `samples/` contains only `InitializeComponent()`. Zero event handlers.

---

## ❌ 3. Do Not Use Service Locator / Splat

```csharp
// ❌ WRONG
var service = Locator.Current.GetService<IMyService>();

// ✅ RIGHT — constructor injection
public class MyViewModel(IMyService service) : ReactiveObject { }
```

**Why**: No `Locator.Current` or `Splat` references exist. DI is purely `Microsoft.Extensions.DependencyInjection` with constructor injection.

**Evidence**: `grep -r "Locator.Current" . --include="*.cs"` → empty. `grep -r "using Splat" . --include="*.cs"` → empty.

---

## ❌ 4. Do Not Throw Exceptions for Control Flow

```csharp
// ❌ WRONG
public async Task<string> GetData()
{
    var result = await FetchFromApi();
    if (result == null) throw new InvalidOperationException("No data");
    return result;
}

// ✅ RIGHT
public async Task<Result<string>> GetData()
{
    var result = await FetchFromApi();
    return result != null
        ? Result.Success(result)
        : Result.Failure<string>("No data available");
}
```

Exceptions are reserved for truly exceptional cases (null guards, not-implemented stubs). All expected failure paths use `Result<T>`.

**Evidence**: `Commands.cs:41` (URI parsing → `Result.Failure`), `AdaptivePanel.cs` (layout errors wrapped in Result), `SubwizardDecisionPageViewModel.cs` (cancellation → `Result.Failure`).

---

## ❌ 5. Do Not Suffix Methods with `Async`

```csharp
// ❌ WRONG
private async Task<string> LoadDataAsync() { ... }

// ✅ RIGHT
private async Task<string> LoadData() { ... }
```

**Evidence**: `DialogSampleViewModel.cs` — `OnShowMessage(...)`, `OnShowNotificationWhileDialogOpen(...)`. `WizardViewModel.cs` — `ShowResults(...)`, `ShowSubwizardResults(...)`. Only 1 private `RunAsync` in entire samples/.

---

## ❌ 6. Do Not Use Leading Underscores for Fields

```csharp
// ❌ WRONG
private readonly INavigator _navigator;

// ✅ RIGHT
private readonly INavigator navigator;
```

**Evidence**: 0 underscore-prefixed fields in `samples/`. Only 4 legacy instances in all of `src/` (panel code).

---

## ❌ 7. Do Not Put Complex Logic in Subscribe

```csharp
// ❌ WRONG — business logic buried in callback
observable.Subscribe(x =>
{
    if (x.IsValid)
    {
        SaveToDatabase(x);
        NotifyUser();
    }
    else
    {
        LogError(x);
    }
});

// ✅ RIGHT — logic in the pipeline, Subscribe just activates
observable
    .Where(x => x.IsValid)
    .SelectMany(x => SaveToDatabase(x).ToSignal())
    .Do(_ => NotifyUser())
    .Subscribe()
    .DisposeWith(disposable);
```

**Evidence**: 95%+ of `.Subscribe()` calls in the codebase are parameterless. `WizardViewModel.cs`, `HomeViewModel.cs`, `ProximityRevealBehavior.cs` all use this pattern.

---

## ❌ 8. Do Not Forget Disposal

```csharp
// ❌ WRONG — subscription leak
SomeObservable.Subscribe();

// ✅ RIGHT — tracked for disposal
SomeObservable.Subscribe().DisposeWith(disposable);
```

Every `IDisposable` subscription that outlives a single method must be tracked via `CompositeDisposable` + `DisposeWith()`. ViewModels that subscribe should implement `IDisposable`.

**Evidence**: `WizardViewModel.cs` implements `IDisposable` with `CompositeDisposable`. Behaviors return `CompositeDisposable` from lifecycle overrides.

---

## ❌ 9. Do Not Use null Where Maybe<T> Is Expected

```csharp
// ❌ WRONG
string? result = dialog.ShowAndGetResult(...);
if (result == null) { /* dismissed */ }

// ✅ RIGHT
Maybe<string> result = await dialog.ShowAndGetResult(...);
result.Match(
    value => HandleValue(value),
    () => HandleDismissed());
```

`Maybe<T>` from CSharpFunctionalExtensions is the standard for optional/absent values. Use `.HasValue`, `.Value`, `.Match()`, `.Map()`, `.Bind()`.

**Evidence**: `DialogSampleViewModel.cs`, `WizardViewModel.cs`, `StorageSampleViewModel.cs`.

---

## ❌ 10. Do Not Create Views Without x:DataType

```xml
<!-- ❌ WRONG — no type safety -->
<UserControl x:Class="MyApp.Views.HomeView">

<!-- ✅ RIGHT — enables compile-time binding checks and source-gen discovery -->
<UserControl x:Class="MyApp.Views.HomeView"
             x:DataType="vm:HomeViewModel">
```

`x:DataType` is declared on 80+ AXAML files in the codebase. It enables type-safe bindings and is required for `DataTypeViewLocator` source generation.

**Evidence**: Every sample View and most library controls declare `x:DataType`.

---

## ❌ 11. Do Not Manually Register Section ViewModels

```csharp
// ❌ WRONG — manual registration
services.AddTransient<HomeViewModel>();
services.AddTransient<SettingsViewModel>();
services.AddTransient<AboutViewModel>();

// ✅ RIGHT — source-generated from [Section] attributes
services.AddAllSectionsFromAttributes(logger);
```

The source generator discovers all `[Section]`-decorated types in the assembly and registers them. Manual registration defeats the purpose and risks desynchronization.

**Evidence**: `MinimalShell/App.axaml.cs`, `CompositionRoot.cs`.

---

## ❌ 12. Do Not Imperatively Compute canExecute

```csharp
// ❌ WRONG — imperative enablement
void UpdateCanSave()
{
    SaveCommand.CanExecute = Name != null && Name.Length > 0;
}

// ✅ RIGHT — reactive canExecute from observable state
var canSave = this.WhenAnyValue(x => x.Name)
    .Select(n => !string.IsNullOrEmpty(n));
SaveCommand = ReactiveCommand.CreateFromTask(Save, canSave);
```

**Evidence**: `Page1ViewModel.cs` — `ReactiveCommand.CreateFromTask(..., this.IsValid())`. `GraphWizardSampleViewModel.cs` — `canExecute: start.WhenAnyValue(x => x.Choice).NotNull()`.

---

## ⚠️ 13. Do Not Mix Wizard Hosting Modes Carelessly

SlimWizard and GraphWizard support both dialog and navigation hosting. Choose consistently:

```csharp
// Dialog hosting — wizard appears as a modal overlay
await wizard.ShowInDialog(dialog, "Title");

// Navigation hosting — wizard pages are pushed onto the Navigator stack
await wizard.Navigate(navigator);
```

`[HYPOTHESIS]` Mixing modes (e.g., starting in navigation then showing a sub-wizard in a dialog) should work but may cause unexpected z-order issues. The sub-wizard sample uses navigation for both parent and child.

---

## ⚠️ 14. Do Not Ignore the StyleInclude Order

```xml
<!-- The order matters: FluentTheme FIRST, then Zafiro styles -->
<FluentTheme />
<StyleInclude Source="avares://Zafiro.Avalonia/Styles.axaml" />
<!-- Dialogs styles AFTER core Zafiro (they depend on it) -->
<StyleInclude Source="avares://Zafiro.Avalonia.Dialogs/Styles.axaml" />
```

`[HYPOTHESIS]` Zafiro.Avalonia.Dialogs/Styles.axaml internally includes `avares://Zafiro.Avalonia/Styles.axaml`, so explicit ordering may be redundant. But both samples include core styles first, suggesting it's the intended pattern.
