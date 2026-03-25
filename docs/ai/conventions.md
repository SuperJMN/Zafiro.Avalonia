# Zafiro.Avalonia — Coding Conventions

> Observed conventions from the codebase. Rules marked `[HYPOTHESIS]` are inferred from patterns but not explicitly stated.

---

## C# Conventions

### Naming

| Element | Convention | Example | Evidence |
|---|---|---|---|
| Private fields | camelCase, **no** leading underscore | `private readonly CompositeDisposable disposable` | 0 underscore-prefixed fields in samples; only 4 in all of src/ (legacy panel code) |
| `[Reactive]` backing fields | `private` lowercase | `[Reactive] private string name;` | `Page1ViewModel.cs`, `MasterDetailsSampleViewModel.cs` |
| Methods returning `Task` | **No** `Async` suffix | `Task OnShowMessage(...)` not `OnShowMessageAsync` | `DialogSampleViewModel.cs`, `WizardViewModel.cs` — sole exception: 1 private `RunAsync` in samples |
| Interfaces | `IHaveX` / `IX` pattern | `IHaveHeader`, `IHaveTitle`, `IHaveFooter` | `Zafiro.UI.Navigation` namespace |
| ViewModels | `FooViewModel` | `HomeViewModel`, `Page1ViewModel` | Universal across samples |
| Views | `FooView` (matching ViewModel) | `HomeView`, `Page1View` | Required by `NamingConventionGeneratedViewLocator` |

### Types and Patterns

| Pattern | Convention | Evidence |
|---|---|---|
| ViewModel base | `ReactiveObject` or `ReactiveValidationObject` | 100% of ViewModels in src/ and samples/ |
| Records for DTOs | `record` for immutable data | `record SampleCard(string Name, string Description, string Icon, string Category, Type ViewModelType)` |
| Nullable reference types | Enabled; prefer `Maybe<T>` over null | `string?` is used for nullable; `Maybe<T>` for semantic absence |
| Command return types | `Result<T>` for fallible; `Unit` for void | `ReactiveCommand<Unit, Result<int>>`, `ReactiveCommand<Unit, Maybe<string>>` |
| DI registration | Constructor injection only | `NavigationSampleViewModel(INavigator navigator)` — primary constructors used |
| `IDisposable` ViewModels | `CompositeDisposable` + `DisposeWith` | `WizardViewModel : IDisposable` |

### Code-Behind Rule

View code-behind (`.axaml.cs`) files contain **only**:

```csharp
public partial class HomeView : UserControl
{
    public HomeView() => InitializeComponent();
}
```

No event handlers. No DI. No logic. All behavior lives in the ViewModel or in Behaviors/Converters.

**Evidence**: Every `.axaml.cs` in `samples/` checked — 100% compliance.

---

## AXAML Conventions

### Namespace Declarations

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:local="clr-namespace:MyApp.Views"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="MyApp.Views.HomeView"
             x:DataType="local:HomeViewModel">
```

- `x:DataType` is declared on the root element for type-safe bindings.
- `x:CompileBindings` is **not** globally enabled — only used in ~7% of AXAML files, on a per-file opt-in basis.

### Binding Patterns

```xml
<!-- Standard property binding (most common) -->
{Binding PropertyName}

<!-- Async observable binding (note the ^) -->
{Binding Navigator.Content^}

<!-- Parent binding with type cast (for DataTemplate contexts) -->
{Binding $parent[UserControl].((vm:HomeViewModel)DataContext).NavigateToSample}

<!-- Self binding -->
{Binding $self.Bounds.Width}

<!-- Two-way binding (explicit) -->
{Binding Text, Mode=TwoWay}
```

**Evidence**: `HomeView.axaml`, `MainView.axaml`, `SlimDataGridView.axaml`.

`[HYPOTHESIS]` The `^` operator on `Navigator.Content^` unwraps `IObservable<T>` — this is standard Avalonia reactive binding syntax. The codebase uses it for `INavigator.Content` which is an observable.

### Style Class Usage (observed in samples)

These utility classes appear in `HomeView.axaml` and other sample views:

| Class | Used On | Likely Purpose |
|---|---|---|
| `Size-XS`, `Size-S`, `Size-M`, `Size-XL` | `TextBlock` | Font size presets |
| `Weight-Bold` | `TextBlock` | Font weight |
| `Text-Muted` | `TextBlock` | Reduced opacity/subdued color |
| `Ghost` | `Button`, `EnhancedButton` | Transparent/minimal button style |
| `Card` | `Border`, `OverlayBorder` | Card styling (defined in `Common.axaml`) |
| `Elevate` | `Border` | Box shadow elevation |
| `Expand` | `EnhancedButton` | Stretch to fill (defined in `Button.axaml`) |
| `CenterContent` | `EnhancedButton` | Center-align content |
| `ShowEmptyContent` | `ListBox`, `CardGrid`, `ItemsControl` | Show empty-state placeholder |
| `H1`, `H3` | `TextBlock` | Heading sizes |

`[HYPOTHESIS]` `Size-*`, `Weight-*`, `Text-Muted`, `Ghost`, `Elevate`, `H1`, `H3` are likely defined in the consuming app's styles or in FluentAvalonia, not in `Zafiro.Avalonia/Styles/`. The library's `Common.axaml` only defines `Card` on `Border`/`OverlayBorder`. `Expand` is defined in `Button.axaml`. Verify where these classes originate before relying on them.

### EnhancedButton Role Classes (confirmed in library)

Defined in `src/Zafiro.Avalonia/Controls/EnhancedButton.axaml`:

- `Primary` — accent-colored
- `Secondary` — subdued
- `Cancel` — cancel action
- `Destructive` — danger/delete action
- `Info` — informational
- `Ghost` — `[HYPOTHESIS]` likely transparent background
- `Hollow` — `[HYPOTHESIS]` outline-only style

### App.axaml Structure

```xml
<Application.Styles>
    <!-- 1. Base theme -->
    <FluentTheme />

    <!-- 2. Zafiro styles (REQUIRED) -->
    <StyleInclude Source="avares://Zafiro.Avalonia/Styles.axaml" />

    <!-- 3. Optional: Dialog styles -->
    <StyleInclude Source="avares://Zafiro.Avalonia.Dialogs/Styles.axaml" />

    <!-- 4. Optional: DataViz styles -->
    <StyleInclude Source="avares://Zafiro.Avalonia.DataViz/Styles.axaml" />
</Application.Styles>

<Application.DataTemplates>
    <!-- 1. Library DataTemplates (wizards, icons, etc.) -->
    <misc:DataTemplateInclude Source="avares://Zafiro.Avalonia/DataTemplates.axaml" />

    <!-- 2. App-specific DataTemplates -->
    <DataTemplate DataType="views:MessageDialogViewModel">
        <MessageDialogView />
    </DataTemplate>

    <!-- 3. Source-generated view locators -->
    <DataTypeViewLocator />
    <NamingConventionGeneratedViewLocator />
</Application.DataTemplates>
```

The order matters: styles are applied in declaration order, and DataTemplates resolve first-match.

**Evidence**: `TestApp/App.axaml`, `MinimalShell/App.axaml`.

---

## DI Registration Conventions

```csharp
var services = new ServiceCollection();

// 1. Shell (if using section-based navigation)
services.AddZafiroShell(logger: logger);

// 2. Auto-discover [Section] ViewModels (source-generated)
services.AddAllSectionsFromAttributes(logger);

// 3. Services
services.AddSingleton(DialogService.Create());
services.AddSingleton<ILauncherService, LauncherService>();
services.AddSingleton<INotificationService>(new NotificationService());

// 4. Navigator
services.AddSingleton<INavigator>(sp =>
    new Navigator(sp, logger.AsMaybe(), RxApp.MainThreadScheduler));

// 5. ViewModels
services.AddTransient<MainViewModel>();

// 6. Platform services
services.AddSingleton<IFileSystemPicker>(_ =>
    new AvaloniaFileSystemPicker(TopLevel.GetTopLevel(view).StorageProvider));

var provider = services.BuildServiceProvider();
```

**Evidence**: `CompositionRoot.cs`.

---

## Rx Pipeline Style

### Preferred: Logic in pipeline, empty Subscribe

```csharp
// ✅ Good — side effects via Do(), SelectMany(), BindTo()
someObservable
    .SelectMany(x => DoWork(x).ToSignal())
    .Subscribe()                               // empty — just activates
    .DisposeWith(disposable);

// ✅ Good — DynamicData bind pattern
items.Connect()
    .Filter(filter)
    .SortBy(x => x.Name)
    .Bind(out filtered)
    .Subscribe();                              // empty — just activates
```

### Discouraged: Logic inside Subscribe

```csharp
// ⚠️ Avoid — complex logic in callback
someObservable.Subscribe(x =>
{
    if (x.IsValid) { DoThingA(); }
    else { DoThingB(); }
});
```

**Evidence**: 95%+ of `.Subscribe()` calls in the codebase are parameterless. The sole exceptions are trivial single-line callbacks or demo/animation code.

---

## Result/Maybe Idiomatic Style

**Always prefer functional combinators over imperative checks.** This is a core convention.

### Result<T> — compose with combinators

```csharp
// ✅ Preferred — pipeline
return await GetHost().ToResult("No host")
    .Map(host => host.Launcher)
    .Bind(l => Result.Try(() => l.LaunchUriAsync(uri)))
    .Ensure(ok => ok, "Launch failed");

// ❌ Avoid — imperative unpacking
var result = await GetHost();
if (result.IsFailure) return Result.Failure(result.Error);
var host = result.Value;  // never do this
```

### Maybe<T> — compose with combinators

```csharp
// ✅ Preferred — Match/Execute/GetValueOrDefault
maybe.Match(v => $"Got: {v}", () => "Empty");
maybe.Execute(v => Save(v));
maybe.GetValueOrDefault("fallback");

// ❌ Avoid — if/HasValue/Value
if (maybe.HasValue) DoSomething(maybe.Value);
return maybe.HasValue ? maybe.Value : "default";
```

Prefer `Map`/`Bind` for transforms, `Tap`/`Execute` for side-effects, `Match` for exhaustive folds, `GetValueOrDefault` for extraction, `Ensure`/`Where` for filtering. See `docs/ai/anti-patterns.md` §15 for the full combinator reference.

**Evidence**: `LauncherService.cs`, `Commands.cs`, `EnhancedButton.axaml.cs`, `DataTemplateInclude.cs`, `NamingConventionViewLocator.cs` all demonstrate clean pipelines. Some older code (`AdaptivePanel.cs`, `GraphWizardBuilderGeneric.cs`) still uses imperative checks — these are legacy patterns, not examples to follow.

---

## Git and Versioning

- **GitVersion** with semver suffix in squash-merge commit messages
- Commit messages: `+semver:major`, `+semver:minor`, `+semver:patch`
- PRs: explanatory message, no boilerplate, squash merge
- Language: English for code, comments, and commits
- Co-authored-by trailer for Copilot-generated commits

**Evidence**: `AGENTS.md`, `WARP.md`.

---

## Responsive Layout Conventions

### Panel Selection Guide

| Scenario | Panel | Why |
|---|---|---|
| Toolbar / nav bar / button row | `FlexPanel` with `Direction="Row"` | Auto + stretch mixing, `MarginLeftAuto` for push-right |
| Cards / tiles that reflow by screen width | `BootstrapGridPanel` | Per-breakpoint column spans, just like Bootstrap |
| App-level structure (sidebar + content) | `SemanticPanel` | Role-based zones, 3 size classes with hysteresis |
| Grid with text-defined template | `BlueprintPanel` | DSL + `LayoutBreakpoint` collection for breakpoints |
| Uniform cards with aspect ratio | `CardPanel` | Auto-columns from aspect ratio + max width |
| Uniform items, no breakpoints needed | `ResponsiveUniformGrid` or `BalancedWrapGrid` | Auto-columns from `MinColumnWidth` |
| Show/hide content based on overflow | `AdaptivePanel` | Swaps Content ↔ OverflowContent automatically |
| Swap entire UI at a width threshold | `ResponsivePresenter` | Narrow/Wide templates with debounced switching |
| True-center with side elements | `TrueCenterPanel` | Center child is truly centered regardless of side widths |

### BootstrapGridPanel Conventions

- **Always set `FluidContainer="True"`** unless you specifically need Bootstrap's fixed max-widths per breakpoint.
- **Start mobile-first**: Set `Col` (base/Xs) first, then override upward: `ColSm`, `ColMd`, etc. Only set breakpoints that change the span.
- **Use Auto (default) sparingly**: `Col="0"` (Auto) splits remaining columns evenly — good for equal-width items, but explicit spans are clearer.
- **Prefer `Gutter` over manual Margin**: The panel handles column and row gaps uniformly.
- **Use `RowBreak` for semantic rows**: Don't rely on overflow wrapping when you want an explicit row boundary.

### FlexPanel Conventions

- **Use `Grow="1"` for the stretchy element**, `Shrink="0"` for fixed elements (buttons, icons).
- **Use `MarginLeftAuto="True"`** to push an element to the right end (like CSS `margin-left: auto`).
- **Prefer `Wrap="Wrap"`** over horizontal scroll for action bars and chip lists.
- **For equal columns**: Set `Grow="1" Shrink="1" Basis="0"` on each child (equivalent to CSS `flex: 1 1 0`).

### Nesting Rules

1. **SemanticPanel at the top** for app structure — never nest SemanticPanels.
2. **BootstrapGridPanel inside content zones** for responsive grids.
3. **FlexPanel inside grid cells** for local auto/stretch row behavior.
4. **Standard panels (StackPanel, Grid, DockPanel)** for leaf-level local layout.

### AXAML Namespace for Panels

```xml
xmlns:z="clr-namespace:Zafiro.Avalonia.Controls.Panels;assembly=Zafiro.Avalonia"
```

**Evidence**: `samples/TestApp/TestApp/Samples/Panels/PanelsView.axaml`, `samples/TestApp/TestApp/Samples/Layout/FlexPanelView.axaml`.
