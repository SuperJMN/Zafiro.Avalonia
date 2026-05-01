# Spec: Responsive MasterDetailsView with Frame Back Integration

## Status

Implemented as a breaking cleanup.

## Context

`MasterDetailsView` currently adapts between a two-pane layout and a compact layout, but its navigation model is too implicit for real application sections hosted inside `Frame`:

- `SelectedItem` must not open details. ViewModels may preselect an item for wide layouts while compact layouts still start on the master route.
- Back navigation must be handled through scoped `IFrameBackParticipant` registration on the nearest `Frame`, not through global registration.
- The hosting `Frame` already owns shell back behavior through `BackCommand`, system back handling, and `AutoHeaderFooterBehavior`. `MasterDetailsView` should participate in that existing frame chrome without the Shell knowing about master/detail.
- The current control owns the master as a `ListBox`. Real sections often need a `DataGrid` or another master presenter on wide widths, and a tappable list on narrow widths.

Reference scenario from ProyectoAna, section "Notas":

- Wide space: show student table and criterion detail together.
- Narrow space: show only the students list. Selecting/tapping a student opens the detail view. The user can go back to the list.
- The selected row can already exist because wide layouts need a current detail item. That selection must not automatically open compact details.
- Changing course/class/term hides compact details and returns to the list, while keeping normal selection semantics.

The solution must depend only on available layout size. It must not branch on desktop/mobile/platform.

## Goal

Create a clean, reusable `MasterDetailsView` that:

- Uses a width-based responsive state: wide means master and detail visible together; compact means master route or detail route, never both.
- Separates selection from detail activation.
- Integrates with `Frame` back behavior through a generic nested-navigation contract, not through Shell-specific code and not through a global `MessageBus`.
- Supports custom master presenters while preserving a good default list-based experience.
- Preserves MVVM: no view code-behind logic required in consuming apps.

## Non-goals

- Do not implement a general multi-page navigator inside `MasterDetailsView`.
- Do not make `Frame` depend on `MasterDetailsView` by type.
- Do not use `OperatingSystem.IsAndroid`, `OperatingSystem.IsIOS`, `OnFormFactor`, or similar platform checks for master/detail behavior.
- Do not require section ViewModels to know about compact/wide layout state.

## Existing Files To Inspect

- `src/Zafiro.Avalonia/Controls/MasterDetailsView.axaml`
- `src/Zafiro.Avalonia/Controls/MasterDetailsView.axaml.cs`
- `src/Zafiro.Avalonia/Controls/Navigation/Frame.axaml`
- `src/Zafiro.Avalonia/Controls/Navigation/Frame.axaml.cs`
- `src/Zafiro.Avalonia/Controls/Navigation/AutoHeaderFooterBehavior.cs`
- `src/Zafiro.Avalonia/Controls/ResponsivePresenter.cs`
- `samples/TestApp/TestApp/Samples/MasterDetails/*`
- `samples/TestApp/TestApp/Shell/MainView.axaml`

## Required Behavior

### Layout State

`MasterDetailsView` must compute compact/wide from its own available width.

- Add or keep a `Breakpoint`/`CompactWidth` property.
- `IsCompact` is true when arranged width is below the breakpoint.
- Wide layout:
  - Master and detail are visible at the same time.
  - `SelectedItem` drives the detail content.
  - The control does not contribute a local back action to the host `Frame`.
- Compact layout:
  - If `AreDetailsShown == false`, show the master route only.
  - If `AreDetailsShown == true` and `SelectedItem != null`, show the detail route only.
  - The control contributes a local back action to the host `Frame` only while compact details are shown.

The layout state must be based on the control bounds or equivalent container-query mechanism. It must not use platform or form-factor checks.

### Selection vs Activation

Selection and activation must be separate concepts.

- `SelectedItem` may be set programmatically without opening compact details.
- `OpenDetailsCommand` or `ActivateItemCommand` opens details for an item:
  - If command parameter is non-null, set `SelectedItem` to that item.
  - If command parameter is null, use the current `SelectedItem`.
  - If there is no item, do nothing.
  - Set `AreDetailsShown = true`.
- `CloseDetailsCommand` hides compact details:
  - Set `AreDetailsShown = false`.
  - Do not clear `SelectedItem`.
- `SelectedItem` removal from `ItemsSource` should hide details and clear or move selection using a documented, deterministic rule.

Do not add a compatibility option that opens details from selection. Sections often preselect the first item so wide details have meaningful content, but compact users still need to start from the master list.

### Host Frame Back Integration

Add a generic nested-back contribution mechanism to `Frame`.

The exact names can change, but the architecture should match this shape:

```csharp
public interface IFrameBackParticipant
{
    IObservable<bool> CanHandleBack { get; }
    ICommand BackCommand { get; }
}
```

`Frame` should discover participants scoped to its visual tree. Acceptable designs:

- Participants register/unregister with the nearest ancestor `Frame` during attach/detach.
- A behavior on `Frame` discovers visual descendants implementing `IFrameBackParticipant`.
- An attached property exposes a scoped coordinator from `Frame` to descendants.

Requirements:

- No global `MessageBus`.
- No static process-wide participant collection.
- No Shell type references.
- Multiple participants are allowed; choose the active participant deterministically. Prefer the most recently activated participant that can handle back, or the deepest focused/visual descendant if focus tracking is already available.
- `Frame.BackCommand` remains the external fallback command, typically bound to `Navigator.Back`.
- The `Frame` template back button and system back handling must execute an effective back command:
  1. active nested participant back command, if any can handle back;
  2. otherwise `Frame.BackCommand`.
- `Frame` must raise enablement for the effective back command when nested participant state changes.

`MasterDetailsView` implements or registers an `IFrameBackParticipant`:

- `CanHandleBack` is true only when `IsCompact && AreDetailsShown`.
- `BackCommand` executes `CloseDetailsCommand`.

This makes the Shell oblivious to master/detail. The Shell keeps this existing pattern:

```xml
<Frame Content="{Binding SelectedSection.Value.Navigator.Content^}"
       BackCommand="{Binding SelectedSection.Value.Navigator.Back}">
    <Interaction.Behaviors>
        <nav:AutoHeaderFooterBehavior />
    </Interaction.Behaviors>
</Frame>
```

### Header and Footer Integration

Do not force `MasterDetailsView` to own the `Frame` header/footer.

If the implementation extends frame chrome contribution beyond back, keep it generic and optional. For example:

```csharp
public interface IFrameChromeParticipant : IFrameBackParticipant
{
    IObservable<object?> Header { get; }
    IObservable<object?> Footer { get; }
}
```

Only add this if it is needed and can compose cleanly with existing `IHaveHeader` / `IHaveFooter`. The minimal required integration is nested back behavior.

### Master and Detail Templates

The control must support both simple and advanced consumers.

Required simple API:

```csharp
public IEnumerable? ItemsSource { get; set; }
public object? SelectedItem { get; set; }
public IDataTemplate? ItemTemplate { get; set; }
public IDataTemplate? DetailsTemplate { get; set; }
public object? Header { get; set; }
public object? Footer { get; set; }
```

Required advanced template API:

```csharp
public IDataTemplate? WideMasterTemplate { get; set; }
public IDataTemplate? CompactMasterTemplate { get; set; }
public IDataTemplate? CompactItemTemplate { get; set; }
public IDataTemplate? EmptyTemplate { get; set; }
```

Guidance:

- Default wide master can be a `ListBox`.
- Default compact master can be a scrollable item list where item activation opens details.
- Advanced master templates should receive a context object, not only raw items. The context should expose:
  - `ItemsSource`
  - `SelectedItem`
  - `OpenDetailsCommand`
  - `CloseDetailsCommand`
  - `AreDetailsShown`
  - `IsCompact`
- This context lets a wide template use `DataGrid`, `SlimDataGrid`, `ListBox`, or another master presenter without code-behind.

Example target usage for the ProyectoAna Notas case:

```xml
<MasterDetailsView ItemsSource="{Binding ScoreRows}"
                   SelectedItem="{Binding SelectedScoreRow, Mode=TwoWay}"
                   CompactWidth="720">
    <MasterDetailsView.WideMasterTemplate>
        <DataTemplate>
            <DataGrid ItemsSource="{Binding ItemsSource}"
                      SelectedItem="{Binding SelectedItem, Mode=TwoWay}">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Apellidos" Binding="{Binding Student.LastName}" />
                    <DataGridTextColumn Header="Nombre" Binding="{Binding Student.FirstName}" />
                    <DataGridTextColumn Header="Nota global" Binding="{Binding Total, StringFormat=F2}" />
                </DataGrid.Columns>
            </DataGrid>
        </DataTemplate>
    </MasterDetailsView.WideMasterTemplate>

    <MasterDetailsView.CompactItemTemplate>
        <DataTemplate>
            <EnhancedButton HorizontalAlignment="Stretch"
                            Command="{Binding OpenDetailsCommand}"
                            CommandParameter="{Binding Item}">
                <TextBlock Text="{Binding Item.Student.FullName}" />
            </EnhancedButton>
        </DataTemplate>
    </MasterDetailsView.CompactItemTemplate>

    <MasterDetailsView.DetailsTemplate>
        <DataTemplate>
            <!-- criteria tree / detail editor -->
        </DataTemplate>
    </MasterDetailsView.DetailsTemplate>
</MasterDetailsView>
```

The exact binding shape can differ if the context object is named differently. The important requirement is that a consumer can define a custom wide master and a compact activator list without duplicating master/detail state in the ViewModel.

### State Reset Hooks

Some sections need to hide compact detail when their logical scope changes. The control should support this without requiring code-behind.

Acceptable options:

- Expose a public `CloseDetailsCommand` so ViewModels can call it if they own the control through binding or a command bridge.
- Add a bindable `NavigationKey` property. When it changes, `MasterDetailsView` closes compact details:

```csharp
public object? NavigationKey { get; set; }
```

Example:

```xml
<MasterDetailsView NavigationKey="{Binding SelectedClass.Id}" />
```

Prefer `NavigationKey` because it keeps the consuming ViewModel independent from the control.

### Accessibility and Input

- Compact list items must be keyboard activatable.
- The default item presenter must not rely only on pointer pressed.
- Frame system back and visible back button must produce the same result.
- Details route must not trap keyboard focus after closing.
- Scroll should be available independently for master and details when content is longer than the available area.

## Implementation Guidance

### Replace MessageBus-Based Navigation

The old `RegisterNavigation`, navigator, and collapsed-state helper types are removed from the supported API.

Preferred path:

- Remove old types as part of the breaking cleanup.
- Remove style registration for old navigation controls.
- Document the new Frame participant mechanism as the supported route.

### Frame Changes

Preserve source compatibility for consumers that bind `Frame.BackCommand`.

Internally, `Frame` should have an effective back command used by:

- template back button;
- `TopLevel.BackRequestedEvent`;
- any future shell chrome that asks the frame whether back is available.

Do not require consumers to change this existing XAML:

```xml
<Frame Content="{Binding Navigator.Content^}"
       BackCommand="{Binding Navigator.Back}" />
```

### MasterDetailsView State Model

Use explicit state, not side effects from `SelectedItem`.

Suggested internal state:

- `IsCompact` derived from bounds and breakpoint.
- `AreDetailsShown` mutable compact route state.
- `CanHandleBack = IsCompact && AreDetailsShown`.

When width changes:

- Wide -> compact: preserve `AreDetailsShown`; if false, show master.
- Compact -> wide: show both panes; do not clear `AreDetailsShown` or `SelectedItem`.
- If wide selection changes, do not force `AreDetailsShown = true`.

When `NavigationKey` changes:

- Set `AreDetailsShown = false`.

## Test Plan

Add tests in `test/Zafiro.Avalonia.Tests`.

Minimum ViewModel/control-state tests:

- `SelectedItem_does_not_open_compact_details_by_default`
- `OpenDetailsCommand_selects_item_and_shows_compact_details`
- `CloseDetailsCommand_hides_details_without_clearing_selection`
- `NavigationKey_change_closes_compact_details`
- `Switching_to_wide_does_not_clear_selection_or_route_state`
- `Compact_back_participant_is_active_only_when_details_are_shown`

Minimum Frame integration tests:

- `Frame_uses_nested_back_before_external_back`
- `Frame_falls_back_to_external_back_after_nested_back_closes_details`
- `Frame_does_not_see_participants_outside_its_visual_tree`
- `Detached_participant_is_unregistered`

Minimum sample/manual verification:

- Update `samples/TestApp/TestApp/Samples/MasterDetails` to show:
  - simple list master;
  - custom wide `DataGrid` master;
  - compact list -> detail route;
  - hosted inside a `Frame` where the frame back button closes compact details before leaving the section.

## Acceptance Criteria

- A section hosted in `ShellView` can use `MasterDetailsView` without adding any Shell-specific code.
- On narrow widths, a preselected item does not skip the master list.
- On narrow widths, activating an item opens only the detail route.
- Back from compact detail returns to compact master and preserves selection.
- On wide widths, master and detail are visible simultaneously.
- `Frame.BackCommand` still works for normal navigation when no nested participant can handle back.
- The implementation contains no global `MessageBus` participant registry.
- The implementation contains no platform/form-factor branches for master/detail behavior.
- Consuming views can use a custom master presenter for wide layout and a different compact item activator without duplicating state in the ViewModel.
