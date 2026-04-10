# Zafiro.Avalonia — AI Overview

> Audience: AI code-generation agents (Copilot, Cursor, etc.) working on projects that consume or extend Zafiro.Avalonia.

## What Zafiro.Avalonia Is

A UI component library for **Avalonia 11.3.x** that ships controls, panels, behaviors, converters, dialogs, wizards, and helpers.
It targets Desktop, Mobile (Android / iOS), and Browser (WASM) from a single codebase.

### Core Tenets (observed from source and README)

| Tenet | Evidence |
|---|---|
| **ReactiveUI-first** | Every ViewModel inherits `ReactiveObject`. `[Reactive]` source generators, `ReactiveCommand`, `WhenAnyValue`, DynamicData used throughout. Zero CommunityToolkit.Mvvm usage. |
| **Functional-reactive** | `Result<T>` and `Maybe<T>` (CSharpFunctionalExtensions) are the standard return types for fallible operations. Exceptions are reserved for truly exceptional cases. |
| **Strict MVVM** | All sample `.axaml.cs` code-behind files contain only `InitializeComponent()`. No event handlers, no service locator, no logic in views. |
| **Composition over inheritance** | Extension methods (`.Enhance()`, `.Connect()`, `.Values()`, `.Empties()`), small interfaces (`IHaveHeader`, `IHaveFooter`, `IHaveTitle`), and DI compose behavior. |
| **DI via MS Extensions** | `Microsoft.Extensions.DependencyInjection` throughout. No Splat/Locator. Source generators auto-discover `[Section]` ViewModels. |

## Package Map

| Package | Purpose |
|---|---|
| `Zafiro.Avalonia` | Controls, panels, behaviors, converters, styles, helpers |
| `Zafiro.Avalonia.Dialogs` | `IDialog` system: messages, forms, wizard hosting |
| `Zafiro.Avalonia.DataViz` | Heatmaps, dendrograms, graphs, monitoring, tables |
| `Zafiro.Avalonia.Generators` | Source generators: `NamingConventionGeneratedViewLocator`, `DataTypeViewLocator`, `AddAllSectionsFromAttributes` |
| `Zafiro.Avalonia.Icons.Optris` | FontAwesome + Material Design icons via Optris |
| `Zafiro.Avalonia.Icons.Svg` | SVG-based icon provider |

External dependency `Zafiro.UI` (NuGet) provides core abstractions: `INavigator`, `IShell`, `IEnhancedCommand`, `SlimWizard`, `Result`/`Maybe` Rx extensions, `[Section]`/`[SectionGroup]` attributes.

## App Bootstrap Lifecycle

Every Zafiro.Avalonia app follows this wiring order:

```
App.axaml                          App.axaml.cs
─────────                          ────────────
1. FluentTheme                     1. Register icon providers
2. StyleInclude (Zafiro styles)    2. Build DI container (ServiceCollection)
3. DataTemplateInclude             3. Call this.Connect(viewFactory, vmFactory, windowFactory)
4. ViewLocator(s)                     └─ handles Desktop + SingleView lifetimes
```

### Minimal Example (from MinimalShell sample, 39 lines total C#)

```csharp
// App.axaml.cs
public override void OnFrameworkInitializationCompleted()
{
    IconControlProviderRegistry.Register(new OptrisIconControlProvider(), asDefault: true);

    var services = new ServiceCollection();
    services.AddZafiroShell(logger: logger);
    services.AddAllSectionsFromAttributes(logger);          // source-generated

    var provider = services.BuildServiceProvider();
    var shell = provider.GetRequiredService<IShell>();

    this.Connect(() => new ShellView(), _ => shell, () => new Window { Title = "App" });
    base.OnFrameworkInitializationCompleted();
}
```

```xml
<!-- App.axaml -->
<Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://Zafiro.Avalonia/Styles.axaml" />
</Application.Styles>
<Application.DataTemplates>
    <misc:DataTemplateInclude Source="avares://Zafiro.Avalonia/DataTemplates.axaml" />
    <NamingConventionGeneratedViewLocator />
</Application.DataTemplates>
```

## Key Abstractions

| Abstraction | Type | Purpose |
|---|---|---|
| `INavigator` | Service | Page navigation: `Go<T>()`, `GoBack()`, `SetInitialPage()` |
| `IShell` | Service | Section-based app shell with sidebar |
| `IDialog` | Service | Modal dialogs returning `Maybe<T>` |
| `INotificationService` | Service | Push notifications from ViewModels |
| `IEnhancedCommand<T>` | Command wrapper | `ReactiveCommand` + label + icon + IsBusy tracking |
| `IHaveHeader` / `IHaveFooter` / `IHaveTitle` | ViewModel contract | Reactive `IObservable<object>` content, auto-consumed by `Frame` + `AutoHeaderFooterBehavior` |
| `[Section]` / `[SectionGroup]` | Attribute | Auto-register ViewModel as a shell section via source generator |
| `Result<T>` / `Maybe<T>` | Functional types | Explicit success/failure and presence/absence — no exceptions for control flow |
| `SlimWizard<T>` | Wizard (linear) | Fluent builder: `WizardBuilder.StartWith().Then().Build()` |
| `GraphWizard<T>` | Wizard (branching) | Graph-based: `GraphWizard.For<T>().Step().Next().Build()` |

## View Resolution Order

Views are resolved in this priority chain (first match wins):

1. **Explicit `DataTemplate`** with `DataType` in `App.axaml`
2. **`DataTemplateInclude`** — imports templates from library assemblies (`avares://`)
3. **`DataTypeViewLocator`** — source-generated from `x:DataType` declarations in `.axaml` files
4. **`NamingConventionGeneratedViewLocator`** — source-generated: `FooViewModel` → `FooView`

## File Organization Conventions (observed)

```
src/Zafiro.Avalonia/
├── Controls/           # TemplatedControl subclasses + their .axaml styles
├── Panels/             # Custom layout panels
├── Behaviors/          # Avalonia behaviors (Xaml.Behaviors)
├── Converters/         # IValueConverter implementations
├── MarkupExtensions/   # {Icon}, {Parse}, {ItemIndex}, etc.
├── Styles/             # Style-only .axaml files (no code-behind)
├── Services/           # ILauncherService, NotificationService
├── Storage/            # IFileSystemPicker, AvaloniaFileSystemPicker
├── ViewLocators/       # NamingConventionViewLocator, DataTypeViewLocator
├── GraphWizard/        # Graph-based wizard system
│   ├── Core/           # Interfaces + logic
│   ├── Builder/        # Fluent builder API
│   └── View/           # AXAML views
├── Styles.axaml        # Master style entry point (includes all sub-styles)
├── DataTemplates.axaml # Global DataTemplates for library types
└── Icons.axaml         # Built-in icon resources

samples/
├── MinimalShell/       # Simplest shell app (~60 lines total)
├── TestApp/            # Full-featured multi-platform demo (40+ samples)
│   ├── TestApp/        # Shared library
│   ├── TestApp.Desktop/
│   ├── TestApp.Browser/
│   ├── TestApp.Android/
│   └── TestApp.iOS/
└── FileExplorer/       # File system explorer demo
```
