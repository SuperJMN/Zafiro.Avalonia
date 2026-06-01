# Zafiro.Avalonia.Templates

`dotnet new` templates for building Avalonia apps with the [Zafiro toolkit](https://github.com/SuperJMN/Zafiro.Avalonia).

## Install

```bash
dotnet new install Zafiro.Avalonia.Templates
```

## Templates

| Short name      | Description                                                                  |
|-----------------|------------------------------------------------------------------------------|
| `zafiro-shell`  | Cross-platform Avalonia app (Desktop, Browser, Android, iOS) with the hierarchical Zafiro Shell, section auto-discovery via `[Section]`, MVVM via ReactiveUI. |

## Usage

```bash
dotnet new zafiro-shell -n MyApp
cd MyApp
dotnet run --project MyApp.Desktop
```

### Options

| Option                     | Description                                                  | Default                       |
|----------------------------|--------------------------------------------------------------|-------------------------------|
| `-n, --name`               | Application/solution name (also used as namespace).          | `ZafiroShellTemplate`         |
| `--application-id`         | Android `ApplicationId` and iOS `CFBundleIdentifier`.        | `com.example.<name-lowercase>`|

## What you get

- Multi-project solution (`.slnx`):
  - **README.md** — concise guide to the generated section tree, `ParentId`, two-level shell rendering, and per-section navigation scopes.
  - **Shared head** (`MyApp/`) — `App.axaml`, `App.axaml.cs` bootstrap with `AddZafiroShell` + `AddAllSectionsFromAttributes`, plus `Sections/` with a two-level default tree (`Home`, `Funds`, `Investor > Find Projects/Funded`, `Founder > My Projects/Funders`) — each section is a ViewModel + View pair tagged with `[Section]` so it is auto-discovered and added to the shell.
  - **MyApp.Desktop** — Windows / Linux / macOS via `Avalonia.Desktop`.
  - **MyApp.Browser** — WebAssembly via `Avalonia.Browser`.
  - **MyApp.Android** — `Avalonia.Android`.
  - **MyApp.iOS** — `Avalonia.iOS`.
- Central Package Management (`Directory.Packages.props`) with pinned Avalonia 12 + Zafiro versions.
- Compiled bindings on by default.
- `ReactiveUI.Avalonia` as the MVVM integration.
- `Zafiro.Avalonia.Mcp.AppHost` wired into Desktop through `.UseMcpDiagnosticsIfDebug()` so Debug builds expose MCP diagnostics while Release builds stay clean.

## Adding a new section

Create a ViewModel + View pair anywhere under the shared head and decorate the ViewModel with `[Section]`. Use an explicit id as the first argument; child sections point to that id through `ParentId`.

```csharp
[Section("funded", "fa-circle-check", sortIndex: 1, FriendlyName = "Funded", ParentId = "investor")]
public partial class FundedViewModel : ReactiveObject { /* ... */ }
```

The Zafiro source generator will register it automatically — no DI plumbing required.

## Uninstall

```bash
dotnet new uninstall Zafiro.Avalonia.Templates
```
