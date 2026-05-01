# MasterDetailsView

> Consumer guide for building responsive master/detail screens with Zafiro.Avalonia.

`MasterDetailsView` owns the responsive layout and the compact detail route. Consumers provide data and templates; ViewModels keep normal selection state and do not need desktop/mobile flags, back handlers, or code-behind.

## When To Use It

Use `MasterDetailsView` when a screen has:

- a collection of items;
- a selected item;
- an editor or read-only details area for that item;
- one layout on wide surfaces and a list-to-details flow on narrow surfaces.

The control decides compact vs wide from its own available width through `CompactWidth`. It does not use platform checks. Desktop, mobile, browser, and embedded layouts all follow the same rule.

## Behavior

Wide layout:

- the master and details are visible at the same time;
- `SelectedItem` drives the detail content;
- selecting an item does not create a nested navigation entry;
- the nearest `Frame` back command remains the normal navigator back command.

Compact layout:

- the master list is shown first;
- setting `SelectedItem` does not open details;
- activating an item through the default compact item list opens details;
- `Frame` back and system back close the details route before the outer navigator goes back.

This separation is intentional: ViewModels can preselect the first item for wide layouts without forcing mobile users directly into details.

## Basic Usage

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="clr-namespace:Zafiro.Avalonia.Controls;assembly=Zafiro.Avalonia"
             xmlns:local="clr-namespace:MyApp.Features.Tasks"
             x:Class="MyApp.Features.Tasks.TasksView"
             x:DataType="local:TasksViewModel">
    <controls:MasterDetailsView ItemsSource="{Binding Tasks}"
                                SelectedItem="{Binding SelectedTask, Mode=TwoWay}"
                                CompactWidth="720"
                                MasterPaneWidth="320">
        <controls:MasterDetailsView.ItemTemplate>
            <DataTemplate DataType="local:TaskItem">
                <TextBlock Text="{Binding Title}" />
            </DataTemplate>
        </controls:MasterDetailsView.ItemTemplate>

        <controls:MasterDetailsView.CompactItemTemplate>
            <DataTemplate DataType="local:TaskItem">
                <Grid RowDefinitions="Auto Auto" ColumnDefinitions="*,Auto">
                    <TextBlock Text="{Binding Title}" FontWeight="SemiBold" />
                    <TextBlock Grid.Column="1" Text="{Binding Status}" />
                    <TextBlock Grid.Row="1" Grid.ColumnSpan="2" Text="{Binding Summary}" />
                </Grid>
            </DataTemplate>
        </controls:MasterDetailsView.CompactItemTemplate>

        <controls:MasterDetailsView.DetailsTemplate>
            <DataTemplate DataType="local:TaskItem">
                <StackPanel Spacing="8">
                    <TextBox Text="{Binding Title, Mode=TwoWay}" />
                    <TextBox Text="{Binding Summary, Mode=TwoWay}"
                             AcceptsReturn="True"
                             TextWrapping="Wrap" />
                </StackPanel>
            </DataTemplate>
        </controls:MasterDetailsView.DetailsTemplate>
    </controls:MasterDetailsView>
</UserControl>
```

The default wide master is a `ListBox`. The default compact master wraps each item template in an activation button, so the compact route opens without extra commands in the ViewModel.

## ViewModel Shape

Keep the ViewModel focused on application state:

```csharp
public partial class TasksViewModel : ReactiveObject
{
    [Reactive] private TaskItem? selectedTask;

    public IReadOnlyList<TaskItem> Tasks { get; } = LoadTasks();
}
```

Do not add `IsCompact`, `AreDetailsShown`, `ShowDetails`, `HideDetails`, or platform-specific navigation state to the ViewModel. Those are control concerns.

## Custom Wide Master

Use `WideMasterTemplate` when the master is not a simple list, for example a `DataGrid`. The template receives a `MasterDetailsViewContext`.

```xml
<controls:MasterDetailsView ItemsSource="{Binding ScoreRows}"
                            SelectedItem="{Binding SelectedScoreRow, Mode=TwoWay}"
                            CompactWidth="720"
                            MasterPaneWidth="360">
    <controls:MasterDetailsView.WideMasterTemplate>
        <DataTemplate DataType="controls:MasterDetailsViewContext">
            <DataGrid Width="{Binding MasterPaneWidth}"
                      ItemsSource="{Binding ItemsSource}"
                      SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
                      AutoGenerateColumns="False">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Last name" Binding="{Binding Student.LastName}" />
                    <DataGridTextColumn Header="First name" Binding="{Binding Student.FirstName}" />
                    <DataGridTextColumn Header="Total" Binding="{Binding Total, StringFormat=F2}" />
                </DataGrid.Columns>
            </DataGrid>
        </DataTemplate>
    </controls:MasterDetailsView.WideMasterTemplate>
</controls:MasterDetailsView>
```

`MasterDetailsViewContext` exposes:

- `ItemsSource`;
- `SelectedItem`;
- `OpenDetailsCommand`;
- `CloseDetailsCommand`;
- `AreDetailsShown`;
- `IsCompact`;
- `HasItems`;
- `HasSelectedItem`;
- `MasterPaneWidth`;
- `ItemTemplate`;
- `CompactItemTemplate`;
- `EmptyTemplate`.

Use `CompactMasterTemplate` only when the default compact list is not enough. In that case the template also receives `MasterDetailsViewContext`, and item activation should call `OpenDetailsCommand` with the item as command parameter.

## Empty State

`EmptyTemplate` receives `MasterDetailsViewContext` and is shown when there is no selected item in wide details or no items in compact master.

```xml
<controls:MasterDetailsView.EmptyTemplate>
    <DataTemplate DataType="controls:MasterDetailsViewContext">
        <TextBlock Classes="Text-Muted" Text="No item selected" />
    </DataTemplate>
</controls:MasterDetailsView.EmptyTemplate>
```

## Frame Integration

`MasterDetailsView` registers with the nearest `Frame` through `IFrameBackParticipant`.

```xml
<Frame Content="{Binding Navigator.Content^}"
       BackCommand="{Binding Navigator.Back}">
    <Interaction.Behaviors>
        <nav:AutoHeaderFooterBehavior />
    </Interaction.Behaviors>
</Frame>
```

No extra wiring is needed in the host shell. When compact details are open, the frame's effective back command closes them. When they are closed, the frame falls back to `Navigator.Back`.

## NavigationKey

Set `NavigationKey` to the logical scope that should reset the compact detail route. Typical examples are selected workspace, selected class, selected term, or route id.

```xml
<controls:MasterDetailsView ItemsSource="{Binding ScoreRows}"
                            SelectedItem="{Binding SelectedScoreRow, Mode=TwoWay}"
                            NavigationKey="{Binding SelectedClass}"
                            CompactWidth="720" />
```

When the key changes, compact details close and the user returns to the master route. The selection is not cleared unless the selected item leaves `ItemsSource`.

## Migration Notes

Prefer this replacement:

- bind `ItemsSource` to the collection;
- bind `SelectedItem` two-way to the current item;
- move the detail UI into `DetailsTemplate`;
- move the wide list/table into `WideMasterTemplate` only if the default `ListBox` is not enough;
- move the mobile row UI into `CompactItemTemplate`;
- use `NavigationKey` for course/class/filter changes.

Remove view-level or ViewModel-level state that only exists to simulate the compact route:

- `AreDetailsShown`;
- `OpenDetails` / `ShowDetails` / `HideDetails`;
- `BackTriggerBehavior` for this route;
- duplicated `ResponsivePresenter` wide/narrow branches for the same master/detail screen;
- platform checks.

Keep application commands such as `Add`, `Delete`, `Save`, `Move`, or `Reload`. They remain ViewModel responsibilities.
