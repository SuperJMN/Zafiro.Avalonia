# Zafiro.Avalonia — Core Concepts

> Reference for AI agents generating Zafiro.Avalonia code. Each concept includes the evidence source and, where the codebase is ambiguous, a `[HYPOTHESIS]` tag.

---

## 1. ReactiveUI Integration

Zafiro.Avalonia is built entirely on ReactiveUI. There is zero CommunityToolkit.Mvvm usage.

### ViewModels

```csharp
// Base class: always ReactiveObject (or ReactiveValidationObject for forms)
public partial class MyViewModel : ReactiveObject
{
    [Reactive] private string name;          // source-generated property via ReactiveUI.SourceGenerators
    [Reactive] private int? count;
}

// With validation
public partial class FormViewModel : ReactiveValidationObject
{
    [Reactive] private string email;

    public FormViewModel()
    {
        this.ValidationRule(x => x.Email, e => !string.IsNullOrEmpty(e), "Required");
    }
}
```

**Evidence**: Every ViewModel in both `src/` and `samples/` inherits `ReactiveObject`. The `[Reactive]` attribute is from `ReactiveUI.SourceGenerators` (not CommunityToolkit).

### Commands

```csharp
// Simple command
var save = ReactiveCommand.CreateFromTask(() => SaveData());

// Command with canExecute derived from observable state
var submit = ReactiveCommand.CreateFromTask(
    () => SubmitForm(),
    this.IsValid());   // IObservable<bool> from validation

// Enhanced command (adds label, icon, IsBusy tracking)
var enhanced = save.Enhance("Save", name: "save-action");
// enhanced.IsBusy   → true while executing
// enhanced.CanExecute → enablement state
// enhanced.Text     → "Save"
```

**Evidence**: `EnhancedCommand.cs` in libs/Zafiro, `Page1ViewModel.cs`, `DialogSampleViewModel.cs`, `WizardViewModel.cs`.

### Property Observation

```csharp
// WhenAnyValue for reactive property changes
this.WhenAnyValue(x => x.SearchText)
    .Throttle(TimeSpan.FromMilliseconds(300), RxApp.MainThreadScheduler)
    .Select(BuildFilter);

// Combining multiple properties
searchFilter.CombineLatest(categoryFilter, (sf, cf) =>
    new Func<Item, bool>(item => sf(item) && cf(item)));
```

**Evidence**: `HomeViewModel.cs`, `Step1ViewModel.cs`, `GraphWizardSampleViewModel.cs`.

---

## 2. Functional Types: Result<T> and Maybe<T>

`CSharpFunctionalExtensions` types are the standard for fallible and optional returns. **Always use the idiomatic combinators** (`Map`, `Bind`, `Match`, `Tap`, `Execute`, etc.) — never inspect `.IsSuccess`/`.HasValue`/`.Value` imperatively.

### Result<T> — Explicit Success/Failure

```csharp
// Constructing results
Result.Success(42)
Result.Failure<int>("Something went wrong")
Result.Try(() => riskyOperation())          // exception-safe wrapping

// ✅ Idiomatic pipeline — chain operations without unpacking
return await GetTopLevel().ToResult("Cannot get host")
    .Map(topLevel => topLevel.Launcher)
    .EnsureNotNull("Launcher unavailable")
    .Bind(launcher => Result.Try(() => launcher.LaunchUriAsync(uri)))
    .Ensure(ok => ok, "Launch failed");

// ✅ Composing fallible operations
return await storage.GetDirectory(path)
    .Map(dir => dir.GetChildren())
    .Bind(children => ProcessAll(children));

// ✅ Side-effects without unwrapping
await clipboard.ToResult("No clipboard")
    .Tap(cb => cb.SetTextAsync(text))
    .Tap(() => notificationService.Show(null!, "Copied"));
```

Key `Result<T>` combinators: `Map`, `Bind`, `Tap`, `OnFailure`, `Ensure`, `EnsureNotNull`, `Match`, `MapError`, `CompensateFailure`, `Result.Try`, `Result.Combine`.

### Maybe<T> — Explicit Presence/Absence

```csharp
// Constructing Maybe
Maybe.From(possiblyNullValue)               // wrap nullable
Maybe<string>.None                          // explicit absence
items.TryFirst(x => x.IsActive)            // from collection

// ✅ Idiomatic — use combinators, not if/HasValue
maybe.Match(
    value => $"Got: {value}",
    () => "Nothing");

maybe.Execute(value => Save(value));        // side-effect if present
maybe.ExecuteNoValue(() => LogMissing());   // side-effect if absent
maybe.GetValueOrDefault("fallback");        // extract with default

// ✅ Pipeline composition
Maybe.From(command)
    .Bind(ToReactiveCommand)
    .Map(rc => rc.IsExecuting.StartWith(false))
    .GetValueOrDefault(Observable.Return(false));

// ✅ Rx extensions for splitting Maybe streams
command.Values()     // IObservable<T> — only present values
command.Empties()    // IObservable<Unit> — only absent values

// ✅ Dialog results use Maybe
Maybe<string> result = await dialog.ShowAndGetResult(viewModel, "Title",
    vm => vm.IsValid(), vm => vm.Text);
result.Match(
    value => HandleResult(value),
    () => HandleDismissal());
```

Key `Maybe<T>` combinators: `Map`, `Bind`, `Match`, `Execute`, `ExecuteNoValue`, `GetValueOrDefault`, `Where`, `TryFirst`, `Maybe.From`.

**Evidence**: `LauncherService.cs`, `Commands.cs`, `EnhancedButton.axaml.cs`, `DataTemplateInclude.cs`, `StorageSampleViewModel.cs`, `DialogSampleViewModel.cs`, `WizardViewModel.cs`.

---

## 3. Shell and Section Navigation

### Shell with [Section] Attributes

```csharp
using Zafiro.UI.Shell.Utils;

[Section(icon: "fa-home", sortIndex: 0)]
public class HomeViewModel
{
    public string Greeting => "Welcome!";
}

[Section(icon: "fa-gear", sortIndex: 1)]
[SectionGroup("settings", "Settings")]
public class SettingsViewModel { }
```

The source generator (`Zafiro.Avalonia.Generators`) discovers `[Section]`-decorated types and generates `AddAllSectionsFromAttributes()` extension method on `IServiceCollection`.

```csharp
services.AddZafiroShell(logger: logger);
services.AddAllSectionsFromAttributes(logger);
```

**Evidence**: `MinimalShell/App.axaml.cs`, `MinimalShell/Sections/*.cs`, `CompositionRoot.cs`.

### Navigator-Based Navigation

```csharp
public class MainViewModel(INavigator navigator)
{
    public INavigator Navigator { get; } = navigator;
}

// Navigate to a ViewModel resolved from DI
await navigator.Go<TargetViewModel>();

// Navigate to a factory-created instance
await navigator.Go(() => new DetailViewModel(item));

// Go back
await navigator.GoBack();
```

AXAML wiring with `Frame`:

```xml
<Frame Content="{Binding Navigator.Content^}"
       BackCommand="{Binding Navigator.Back}">
    <Interaction.Behaviors>
        <nav:AutoHeaderFooterBehavior />
    </Interaction.Behaviors>
</Frame>
```

`AutoHeaderFooterBehavior` inspects the current ViewModel for `IHaveHeader` / `IHaveFooter` and auto-populates the Frame's header/footer.

**Evidence**: `MainView.axaml`, `MainViewModel.cs`, `NavigationSampleViewModel.cs`.

### Reactive Content Interfaces

```csharp
public class AboutViewModel : IHaveHeader, IHaveFooter, IHaveTitle
{
    public IObservable<object> Header => Observable.Return<object>("About");
    public IObservable<object> Footer => Observable.Return("v1.0");
    public IObservable<string> Title => Observable.Return("About Page");
}
```

These are `IObservable<>`, not plain properties — they support dynamic, changing content (e.g., a wizard page title that updates as the user types).

**Evidence**: `AboutViewModel.cs`, `Page1ViewModel.cs` (reactive `Title` from `WhenAnyValue`), `NavigationSampleViewModel.cs`.

---

## 4. Wizard Systems

### SlimWizard (Linear)

```csharp
var wizard = WizardBuilder
    .StartWith(() => new Page1ViewModel(), "Step 1")
        .NextWith(model => model.Continue.Enhance("Next"))
    .Then(result => new Page2ViewModel(result), "Step 2")
        .Next((vm, prev) => (prev, vm.Text!)).WhenValid()
    .Then(_ => new Page3ViewModel(), "Done")
        .Next((_, val) => val, "Close").WhenValid()
    .Build(StepKind.Completion);

// Show in dialog (returns Maybe<T>)
Maybe<TResult> result = await wizard.ShowInDialog(dialog, "Wizard Title");

// Show as navigation page
Maybe<TResult> result = await wizard.Navigate(navigator);
```

Wizard pages implement `ReactiveValidationObject` for validation gating. The `.WhenValid()` call gates the Next button on `this.IsValid()`.

**Evidence**: `WizardViewModel.cs`, `Page1ViewModel.cs`, `Page2ViewModel.cs`.

### GraphWizard (Branching)

```csharp
var flow = GraphWizard.For<string>();

var endNode = flow.Step(new SummaryVm(), "End")
    .Finish(vm => "done", nextLabel: "Finish!")
    .Build();

var nodeA = flow.Step(new OptionAVm(), "Path A")
    .Next(vm => endNode, nextLabel: "Continue")
    .Build();

var nodeB = flow.Step(new OptionBVm(), "Path B")
    .Next(vm => endNode)
    .Build();

var startNode = flow.Step(new StartVm(), "Start")
    .Next(vm => vm.Choice == "A" ? nodeA : nodeB,
          canExecute: start.WhenAnyValue(x => x.Choice).NotNull(),
          nextLabel: dynamicLabelObservable)
    .Build();

var wizard = new GraphWizard<string>(startNode);
```

**Evidence**: `GraphWizardSampleViewModel.cs`, `GraphWizardGenericSampleViewModel.cs`.

---

## 5. Dialog System

```csharp
// Simple message
await dialog.ShowMessage("Title", "Body text");

// Message with tone
await dialog.ShowMessage("Warning", "Be careful!", icon: "⚠️", tone: DialogTone.Warning);

// Form dialog returning Maybe<T>
Maybe<string> result = await dialog.ShowAndGetResult(
    new FormViewModel(), "Edit", vm => vm.IsValid(), vm => vm.Text);

// Custom options
await dialog.Show("Title", closeable => [
    new Option("OK", ReactiveCommand.Create(closeable.Close).Enhance(),
        new Settings { Icon = "✔️", IsDefault = true })
]);
```

**Evidence**: `DialogSampleViewModel.cs`.

---

## 6. DynamicData for Reactive Collections

```csharp
private readonly SourceCache<Item, string> items = new(c => c.Id);
private readonly ReadOnlyObservableCollection<Item> filtered;

public MyViewModel()
{
    var filter = this.WhenAnyValue(x => x.SearchText)
        .Throttle(TimeSpan.FromMilliseconds(300), RxApp.MainThreadScheduler)
        .Select(BuildFilter);

    items.Connect()
        .Filter(filter)
        .SortBy(c => c.Name)
        .Bind(out filtered)
        .Subscribe();
}

public ReadOnlyObservableCollection<Item> Items => filtered;
```

**Evidence**: `HomeViewModel.cs`.

---

## 7. View Location

Three source-generated view locators work in priority chain:

1. **`DataTemplateInclude`** — imports `DataTemplates.axaml` from library assemblies
2. **`DataTypeViewLocator`** — auto-registers Views that declare `x:DataType` in their AXAML
3. **`NamingConventionGeneratedViewLocator`** — matches `FooViewModel` → `FooView` by convention

```xml
<!-- App.axaml -->
<Application.DataTemplates>
    <misc:DataTemplateInclude Source="avares://Zafiro.Avalonia/DataTemplates.axaml" />
    <DataTypeViewLocator />
    <NamingConventionGeneratedViewLocator />
</Application.DataTemplates>
```

Both `DataTypeViewLocator` and `NamingConventionGeneratedViewLocator` are generated at compile time by `Zafiro.Avalonia.Generators`.

**Evidence**: `TestApp/App.axaml`, `MinimalShell/App.axaml`.

---

## 8. Icon System

```csharp
// Register providers at startup (App.axaml.cs)
IconControlProviderRegistry.Register(new SvgIconControlProvider());
IconControlProviderRegistry.Register(new ProjektankerIconControlProvider(), asDefault: true);
```

```xml
<!-- AXAML: use Projektanker icons directly -->
<i:Icon Value="fa-solid fa-home" />

<!-- Or via Zafiro's {Icon} markup extension -->
<ContentPresenter Content="{Icon fa-wallet}" />
```

Icons are referenced by string keys: `"fa-home"` (FontAwesome), `"mdi-settings"` (Material Design).

**Evidence**: `TestApp/App.axaml.cs`, `HomeView.axaml`, `MinimalShell/Sections/*.axaml`.

---

## 9. Controls Quick Reference

### Layout Controls

| Control | Key Properties | Usage |
|---|---|---|
| `EdgePanel` | `StartContent`, `Content`, `EndContent`, `Spacing` | Three-region horizontal layout |
| `HeaderedContainer` | `Header`, `Content`, `Footer`, `Spacing`, `HeaderClasses`, `ContentClasses` | Content with header/footer |
| `MasterDetailsView` | `Items`, `SelectedItem`, `DetailTemplate` | Side list + detail pane |
| `Frame` | `Content`, `BackCommand`, `Header`, `Footer` | Navigation container |
| `AdaptiveControl` | `PortraitTemplate`, `LandscapeTemplate` | Responsive two-template control |

### Interactive Controls

| Control | Key Properties | Usage |
|---|---|---|
| `EnhancedButton` | `Icon`, `Role`, `Tint`, `Spacing`, `BoxShadow` | Button with icon + role theming |
| `StringEditorControl` | `Field`, `IsEditing`, `IsLocked` | Inline text edit with commit/rollback |
| `Loading` | `IsLoading`, `Content`, `PageTransition` | Loading state with content transition |
| `SlimDataGrid` | `Columns`, `DataRows` | Lightweight data grid |
| `StepIndicator` | `Steps`, `CurrentStep` | Wizard step progress |

### Role Classes for EnhancedButton

`Primary`, `Secondary`, `Cancel`, `Destructive`, `Info`, `Ghost`, `Hollow`

**Evidence**: `EnhancedButton.axaml` (lines 20-24, 102-116), `WizardFooter.axaml`, `HomeView.axaml`.

---

## 10. Custom Panels

| Panel | Purpose | Key Properties |
|---|---|---|
| `FlexBox` | CSS Flexbox-like layout | `Direction`, `Wrap`, `JustifyContent`, `AlignItems`, `Gap` |
| `BootstrapGridPanel` | 12-column responsive grid | `Columns`, `ColumnSpan` (attached) |
| `CardPanel` | Card-sized uniform grid | `MinCardWidth`, `MaxCardWidth`, `Spacing` |
| `SmartGrid` | Auto-configuring Grid | `Columns`, `Rows`, `Spacing` |
| `BalancedWrapGrid` | Balanced column-width wrap | `MaxItemWidth`, `MinItemWidth`, `Spacing` |
| `ProportionalCanvas` | Proportional positioning (0.0-1.0) | `X`, `Y` (attached, proportional) |
| `AdaptivePanel` | Switches templates by size | `PortraitTemplate`, `LandscapeTemplate`, `Breakpoint` |

`[HYPOTHESIS]` The exact property names for `FlexBox` and `BootstrapGridPanel` are inferred from CSS naming conventions. Verify against `src/Zafiro.Avalonia/Panels/FlexBox.cs` and `BootstrapGridPanel.cs`.

---

## 11. Disposal Pattern

```csharp
public class MyViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable disposable = new();

    public MyViewModel()
    {
        SomeObservable
            .SelectMany(x => DoWork(x).ToSignal())
            .Subscribe()
            .DisposeWith(disposable);
    }

    public void Dispose() => disposable.Dispose();
}
```

Every subscription that outlives a single method should be tracked with `DisposeWith(disposable)`. Behaviors use the same pattern, returning `CompositeDisposable` from `OnAttachedToVisualTreeOverride()`.

**Evidence**: `WizardViewModel.cs`, `ProximityRevealBehavior.cs`, 30+ files use this pattern.
