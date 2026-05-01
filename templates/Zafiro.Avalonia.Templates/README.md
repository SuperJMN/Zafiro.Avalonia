# Zafiro.Avalonia.Templates

`dotnet new` templates for building Avalonia apps with the [Zafiro toolkit](https://github.com/SuperJMN/Zafiro.Avalonia).

## Install

```bash
dotnet new install Zafiro.Avalonia.Templates
```

## Templates

| Short name      | Description                                                                  |
|-----------------|------------------------------------------------------------------------------|
| `zafiro-shell`  | Cross-platform Avalonia app (Desktop, Browser, Android, iOS) with the Zafiro Shell, section auto-discovery via `[Section]`, MVVM via ReactiveUI. |

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
  - **Shared head** (`MyApp/`) — `App.axaml`, `App.axaml.cs` bootstrap with `AddZafiroShell` + `AddAllSectionsFromAttributes`, plus `Sections/` (Home / Settings / About) — each section is a ViewModel + View pair tagged with `[Section]` so it is auto-discovered and added to the shell.
  - **MyApp.Desktop** — Windows / Linux / macOS via `Avalonia.Desktop`.
  - **MyApp.Browser** — WebAssembly via `Avalonia.Browser`.
  - **MyApp.Android** — `Avalonia.Android`.
  - **MyApp.iOS** — `Avalonia.iOS`.
- Central Package Management (`Directory.Packages.props`) with pinned Avalonia 12 + Zafiro versions.
- Compiled bindings on by default.
- `ReactiveUI.Avalonia` as the MVVM integration.
- `Zafiro.Avalonia.Mcp.AppHost` wired into Desktop in **Debug** builds (via `.UseMcpDiagnostics()` under `#if DEBUG`) so AI agents and other MCP clients can inspect/drive the running UI.

## Adding a new section

Create a ViewModel + View pair anywhere under the shared head and decorate the ViewModel with `[Section]`:

```csharp
[Section("Profile", "fa-solid fa-user", sortIndex: 3)]
public partial class ProfileViewModel : ReactiveObject { /* ... */ }
```

The Zafiro source generator will register it automatically — no DI plumbing required.

## Uninstall

```bash
dotnet new uninstall Zafiro.Avalonia.Templates
```
