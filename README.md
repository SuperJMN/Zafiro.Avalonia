# Zafiro.Avalonia

[![NuGet](https://img.shields.io/nuget/v/Zafiro.Avalonia?logo=nuget&label=Zafiro.Avalonia)](https://www.nuget.org/packages/Zafiro.Avalonia)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Zafiro.Avalonia?logo=nuget&label=downloads)](https://www.nuget.org/packages/Zafiro.Avalonia)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A UI components library for **Avalonia 11.3.x** that provides controls, dialogs, behaviors, and helpers for desktop, mobile, and browser applications. Built with **ReactiveUI**, a strong **functional-reactive** orientation (`Result<T>`, `Maybe<T>`, `IObservable<T>`), and absolute respect for the **MVVM pattern** — no logic in views, no UI in ViewModels.

## Packages

| Package | NuGet | Description |
|---------|-------|-------------|
| **Zafiro.Avalonia** | [![NuGet](https://img.shields.io/nuget/v/Zafiro.Avalonia?logo=nuget)](https://www.nuget.org/packages/Zafiro.Avalonia) | Core controls, panels, behaviors, converters, and helpers |
| **Zafiro.Avalonia.Dialogs** | [![NuGet](https://img.shields.io/nuget/v/Zafiro.Avalonia.Dialogs?logo=nuget)](https://www.nuget.org/packages/Zafiro.Avalonia.Dialogs) | Dialog system for desktop and mobile |
| **Zafiro.Avalonia.DataViz** | [![NuGet](https://img.shields.io/nuget/v/Zafiro.Avalonia.DataViz?logo=nuget)](https://www.nuget.org/packages/Zafiro.Avalonia.DataViz) | Data visualization (heatmaps, dendrograms, graphs) |
| **Zafiro.Avalonia.Generators** | [![NuGet](https://img.shields.io/nuget/v/Zafiro.Avalonia.Generators?logo=nuget)](https://www.nuget.org/packages/Zafiro.Avalonia.Generators) | Source generator for view locators and section registration |
| **Zafiro.Avalonia.Icons.Projektanker** | [![NuGet](https://img.shields.io/nuget/v/Zafiro.Avalonia.Icons.Projektanker?logo=nuget)](https://www.nuget.org/packages/Zafiro.Avalonia.Icons.Projektanker) | Icon provider using Projektanker (FontAwesome, Material Design) |
| **Zafiro.Avalonia.Icons.Svg** | [![NuGet](https://img.shields.io/nuget/v/Zafiro.Avalonia.Icons.Svg?logo=nuget)](https://www.nuget.org/packages/Zafiro.Avalonia.Icons.Svg) | SVG-based icon provider |

## Getting Started

```bash
dotnet add package Zafiro.Avalonia
```

For dialogs:
```bash
dotnet add package Zafiro.Avalonia.Dialogs
```

For auto-generated view locators and section registrations (recommended):
```bash
dotnet add package Zafiro.Avalonia.Generators
```

## Quick Start

### Application Bootstrap with `Connect`

Wire up your app in one line — works on Desktop, Mobile, and Browser:

```csharp
public override void OnFrameworkInitializationCompleted()
{
    this.Connect(() => new MainView(), view => CompositionRoot.Create(view), () => new MainWindow());
    base.OnFrameworkInitializationCompleted();
}
```

`Connect` handles `IClassicDesktopStyleApplicationLifetime` and `ISingleViewApplicationLifetime` automatically, so the same code runs everywhere.

### Shell, Sections, and Navigation

A section-based navigation system integrated with `Microsoft.Extensions.DependencyInjection`:

```csharp
ServiceCollection services = new();

services.AddSingleton<IShell, Shell>();
services.AddSingleton(DialogService.Create());
services.AddScoped<INavigator>(provider => new Navigator(provider, logger, RxApp.MainThreadScheduler));
services.AddAllSectionsFromAttributes(logger); // auto-discovers [Section] ViewModels
services.AddTransient<MainViewModel>();

var serviceProvider = services.BuildServiceProvider();
return serviceProvider.GetRequiredService<MainViewModel>();
```

Combined with `Zafiro.Avalonia.Generators`, sections are discovered from `[Section]` attributes and registered automatically.

### Dialogs and Wizards

Dialogs that work on **both** desktop and mobile:

```csharp
Result<Maybe<T>> result = await dialog.ShowAndGetResult(viewModel, "Title");
```

https://github.com/SuperJMN-Zafiro/Zafiro.Avalonia/assets/3109851/d3d29a3e-3a35-4b27-abe0-14d95405c651

Build multi-step wizards declaratively with **SlimWizard**:

```csharp
var wizard = WizardBuilder
    .StartWith(() => new Page1ViewModel(), "Step 1")
        .NextWith(model => model.Continue.Enhance("Next"))
    .Then(result => new Page2ViewModel(result), "Step 2")
        .NextWhenValid((vm, prev) => Result.Success(vm.Text!))
    .WithCompletionFinalStep();
```

https://github.com/SuperJMN-Zafiro/Zafiro.Avalonia/assets/3109851/47dad47a-af35-489c-83b7-0a7c853879f7

### EnhancedCommand

Wraps `ReactiveCommand` with UX metadata (text, icon, name) and **busy state** — distinguishing between *busy* (executing) and *disabled* (can't execute) via `IEnhancedCommand`:

```csharp
var command = ReactiveCommand.CreateFromTask(() => DoSomething());
var enhanced = command.Enhance("Save", name: "save");
// enhanced.IsBusy tracks execution; enhanced.CanExecute tracks enablement
```

### View Locators

Automatically resolves Views for ViewModels by naming convention (`MainViewModel` → `MainView`) and by `x:DataType` discovery via source generators:

```csharp
DataTemplates.Add(new NamingConventionViewLocator());
```

With `Zafiro.Avalonia.Generators`, `x:DataType` declarations in `.axaml` files are discovered at compile time and registered automatically.

## Key Features

### Navigation and Shell

- **Shell / ShellView** — Section-based application shell with sidebar navigation.
- **Navigator** — Observable navigation stack integrated with DI.
- **SectionStrip** — Tab-like section navigation with grouping support.
- **Sections auto-registration** — Source generator discovers `[Section]` ViewModels and wires DI.

### Controls

| Control | Description |
|---------|-------------|
| **HeaderedContainer** | Content with header, footer, and configurable spacing |
| **EdgePanel** | Panel with Start, Content, and End regions |
| **EnhancedButton** | Button with icon, role-based theming, and box shadow |
| **Loading** | Loading indicator with content transition |
| **BalancedWrapGrid** | Wrap panel with balanced column widths and `MaxItemWidth` |
| **MasterDetailsView** | Side list with detail panel, responsive layout |
| **AdaptiveControl** | Two templates: portrait and landscape |
| **StepIndicator** | Visual step progress for wizards |

https://github.com/SuperJMN-Zafiro/Zafiro.Avalonia/assets/3109851/13279313-92cc-4ba9-902e-e3a26da87916

### Services

- **DialogService** — Show dialogs from ViewModels without coupling to the View layer.
- **NotificationService** — Push notifications from ViewModels.
- **ILauncherService** — Open URLs and files from ViewModels.

### Commands and Selection

- **EnhancedCommand** — `ReactiveCommand` wrapper with text/icon metadata, busy/disabled distinction via `IEnhancedCommand`.
- **ReactiveSelection** — Observable selection model with multi-select support.
- **CommandPool / EnqueueCommandAction** — Throttled, pooled command execution.

### Helpers

- **`Connect`** — One-line app bootstrap for all platforms (Desktop, Mobile, Browser).
- **NamingConventionViewLocator + DataTypeViewLocator** — Convention and `x:DataType` based ViewModel → View resolution.
- **IconExtension** — Unified icon markup extension supporting Projektanker and SVG providers.
- **ReturnExtension** — Markup extension for returning observables in design-time data.

## Samples

The solution includes runnable samples that demonstrate all features:

```bash
# Desktop
dotnet run --project samples/TestApp/TestApp.Desktop

# Browser (WASM)
dotnet run --project samples/TestApp/TestApp.Browser
```

## Philosophy

- **Functional + Reactive** — `Result<T>`, `Maybe<T>`, and `IObservable<T>` throughout. No exceptions for control flow, explicit error handling everywhere.
- **MVVM purist** — Strict separation: no UI logic in ViewModels, no business logic in Views. Services are injected, never resolved from Views.
- **Composition over inheritance** — Small, composable building blocks and extension methods.
- **ReactiveUI-first** — State as observables, commands for intents, no imperative event handlers.
- **Cross-platform** — Desktop, Android, iOS, and Browser from the same codebase.

## Disclaimer

Zafiro.Avalonia is an independent community project and is not affiliated with, endorsed by, or sponsored by AvaloniaUI OÜ.

Avalonia is a trademark of AvaloniaUI OÜ.

## License

[MIT](LICENSE) © José Manuel Nieto ([@SuperJMN](https://github.com/SuperJMN))
