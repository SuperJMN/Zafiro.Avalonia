# Agent Notes for Zafiro.Avalonia

> This file is the primary entry point for AI coding agents (Copilot, Cursor, etc.).
> Detailed reference docs live in `docs/ai/`. Read them when you need depth; this file gives you the essentials.

## Project Identity

Zafiro.Avalonia is a UI component library for **Avalonia 11.3.x** — controls, panels, behaviors, converters, dialogs, wizards, helpers. Targets Desktop, Mobile (Android/iOS), and Browser (WASM). Built on **ReactiveUI**, **CSharpFunctionalExtensions** (`Result<T>`, `Maybe<T>`), and strict **MVVM**.

## Tech Stack (non-negotiable)

| Layer | Technology | Notes |
|---|---|---|
| MVVM framework | **ReactiveUI** + ReactiveUI.SourceGenerators | `[Reactive]` for properties, `ReactiveCommand` for commands, `WhenAnyValue` for observation. **Never** use CommunityToolkit.Mvvm. |
| Functional types | **CSharpFunctionalExtensions** | `Result<T>` for fallible ops, `Maybe<T>` for optional values. No exceptions for control flow. |
| DI | **Microsoft.Extensions.DependencyInjection** | Constructor injection only. No Splat/Locator. |
| UI framework | **Avalonia 11.3.x** + FluentTheme | `x:DataType` on all views. Code-behind contains only `InitializeComponent()`. |
| Validation | **ReactiveUI.Validation** | `ReactiveValidationObject`, `this.ValidationRule()`, `this.IsValid()` |
| Reactive collections | **DynamicData** | `SourceCache<T,K>`, `.Connect().Filter().SortBy().Bind()` |

## Critical Rules

1. **No logic in code-behind** — `.axaml.cs` files contain only `InitializeComponent()`. All behavior lives in ViewModels, Behaviors, or Converters.
2. **No CommunityToolkit.Mvvm** — Use `ReactiveObject`, `[Reactive]`, `ReactiveCommand` exclusively.
3. **No service locator** — Constructor injection via MS DI. No `Locator.Current`, no `Splat`.
4. **No exceptions for control flow** — Return `Result<T>` / `Maybe<T>`. Reserve `throw` for truly exceptional cases.
5. **Idiomatic Result/Maybe** — Use `Map`, `Bind`, `Match`, `Tap`, `Execute`, `GetValueOrDefault`, `Ensure`, etc. **Never** inspect `.IsSuccess`/`.HasValue`/`.Value` imperatively. See anti-pattern #15 below.
6. **No `Async` suffix** — Methods returning `Task` omit the `Async` suffix.
7. **No leading underscores** — Private fields use `camelCase`: `private readonly INavigator navigator;`
8. **Empty Subscribe** — Put logic in the Rx pipeline (`.Where()`, `.Select()`, `.Do()`, `.SelectMany()`), not in `.Subscribe()` callbacks.
9. **Track subscriptions** — `CompositeDisposable` + `.DisposeWith(disposable)` for any subscription outliving a method.
10. **`x:DataType` on all Views** — Required for type-safe bindings and source-generated view location.
11. **Responsive by default** — All layouts MUST use responsive panels (`FlexPanel` for bars, `BootstrapGridPanel` for grids). Never use fixed `Grid`/`StackPanel`/`UniformGrid` for content that should adapt to screen size. See anti-pattern #16.

## App Bootstrap Pattern

```
App.axaml                          App.axaml.cs
─────────                          ────────────
1. FluentTheme                     1. Register icon providers
2. StyleInclude (Zafiro)           2. Build ServiceCollection
3. DataTemplateInclude             3. this.Connect(view, vm, window)
4. ViewLocator(s)
```

Minimal startup (39 lines total):
```csharp
// App.axaml.cs
services.AddZafiroShell(logger: logger);
services.AddAllSectionsFromAttributes(logger);  // source-generated from [Section]
this.Connect(() => new ShellView(), _ => shell, () => new Window());
```

## Key Abstractions

| Abstraction | Purpose |
|---|---|
| `[Section]` / `[SectionGroup]` | Auto-register ViewModel as shell section via source generator |
| `INavigator` | `Go<T>()`, `GoBack()`, `SetInitialPage()` |
| `IDialog` | `ShowMessage()`, `ShowAndGetResult<T>()` → returns `Maybe<T>` |
| `IEnhancedCommand<T>` | `ReactiveCommand` + label/icon/IsBusy: `.Enhance("Save")` |
| `IHaveHeader` / `IHaveFooter` / `IHaveTitle` | `IObservable<object>` reactive content for Frame/wizards |
| `SlimWizard<T>` | `WizardBuilder.StartWith().Then().Build()` — linear wizard |
| `GraphWizard<T>` | `GraphWizard.For<T>().Step().Next().Build()` — branching wizard |
| `BootstrapGridPanel` | 12-col responsive grid: `Col`/`ColSm`/`ColMd`/`ColLg`/`ColXl`/`ColXxl` per child |
| `FlexPanel` | CSS Flexbox: `Grow`/`Shrink`/`Basis`/`Wrap`/`JustifyContent`/`AlignItems`/`Gap` |
| `SemanticPanel` | App structure: Primary/Secondary/Sidebar/Actions with 3 responsive size classes |

## Canonical Files to Reference

| Concept | File |
|---|---|
| Minimal bootstrap | `samples/MinimalShell/App.axaml.cs` |
| `[Section]` ViewModel | `samples/MinimalShell/Sections/HomeViewModel.cs` |
| Frame + Navigator | `samples/TestApp/TestApp/Shell/MainView.axaml` |
| Full DI | `samples/TestApp/TestApp/CompositionRoot.cs` |
| DynamicData filtering | `samples/TestApp/TestApp/Samples/HomeViewModel.cs` |
| SlimWizard | `samples/TestApp/TestApp/Samples/SlimWizard/WizardViewModel.cs` |
| GraphWizard | `samples/TestApp/TestApp/Samples/GraphWizard/GraphWizardSampleViewModel.cs` |
| Dialog patterns | `samples/TestApp/TestApp/Samples/Dialogs/DialogSampleViewModel.cs` |
| Validation + [Reactive] | `samples/TestApp/TestApp/Samples/SlimWizard/Pages/Page1ViewModel.cs` |
| Responsive layout panels | `samples/TestApp/TestApp/Samples/Panels/PanelsView.axaml` |
| FlexPanel patterns | `samples/TestApp/TestApp/Samples/Layout/FlexPanelView.axaml` |
| AdaptivePanel | `samples/TestApp/TestApp/Samples/Layout/AdaptivePanelView.axaml` |
| ResponsivePresenter | `samples/TestApp/TestApp/Samples/Layout/ResponsivePresenter/ResponsivePresenterView.axaml` |
| Responsive design (ContainerQuery + FlexPanel) | `samples/TestApp/TestApp/Samples/Layout/ResponsiveLayoutsView.axaml` |

## GitVersion Pull Request Workflow

1. Create a branch for the feature.
2. Implement the feature.
3. Push changes.
4. Create a PR with an explanatory message excluding boilerplate, focusing on the global idea and important future details.
5. Wait for CI to pass.
6. Squash merge the PR using GitVersion semver: The squash merge commit message MUST end with the suffix `+semver:[major|minor|fix]` so GitVersion correctly bumps the version.

## Detailed Documentation

For in-depth reference, see `docs/ai/`:

| File | Contents |
|---|---|
| `docs/ai/overview.md` | Architecture, package map, bootstrap lifecycle, file organization |
| `docs/ai/concepts.md` | Core concepts with code examples: ReactiveUI, Result/Maybe, Shell, Navigation, Wizards, Dialogs, DynamicData, View Location, Responsive Layout |
| `docs/ai/conventions.md` | Naming, typing, AXAML patterns, DI registration, Rx pipeline style, style classes, responsive layout conventions |
| `docs/ai/anti-patterns.md` | 16 anti-patterns with wrong/right examples |
| `docs/ai/examples.md` | 11 complete, copy-pasteable canonical examples from the codebase |
