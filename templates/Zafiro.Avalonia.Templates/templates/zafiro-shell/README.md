# ZafiroShellTemplate

This app starts with a two-level Zafiro shell section tree.

## Section Tree

```text
Home
Funds
Investor
  Find Projects
  Funded
Founder
  My Projects
  Funders
```

## How It Is Wired

- `ZafiroShellTemplate/App.axaml.cs` registers the shell with `AddZafiroShell()` and discovers sections with `AddAllSectionsFromAttributes()`.
- The app resolves `IHierarchicalShell` and gives it directly to `ShellView`.
- Root sections are the ViewModels with `[Section]` and no `ParentId`.
- Child sections use `ParentId` to point at the explicit id of another section, for example `ParentId = "investor"`.
- `SectionGroup` is only presentation metadata; it does not create hierarchy.

## Runtime Behavior

`ShellView` represents two navigation levels by default. On desktop, root sections live in the sidebar and the active root's children are nested below it. On compact layouts, root sections move to the bottom bar and the active child level appears as tabs above the content.

When a parent section has visible children, selecting the parent opens its remembered child, or the first visible child if there is no remembered child yet. The parent ViewModel and View still exist so the section is complete, but the default user-facing navigation focuses on the leaf section.

Each section owns its own `INavigator`. Switching between sections preserves the internal navigation state of each section.

## Adding Sections

Add a ViewModel and a matching View under `ZafiroShellTemplate/Sections/`, then decorate the ViewModel:

```csharp
[Section("analytics", "fa-chart-line", 4, FriendlyName = "Analytics")]
public class AnalyticsViewModel
{
}

[Section("traffic", "fa-route", 0, FriendlyName = "Traffic", ParentId = "analytics")]
public class TrafficViewModel
{
}
```

Use stable, explicit ids. `ParentId` must match another section id, not a class name.
