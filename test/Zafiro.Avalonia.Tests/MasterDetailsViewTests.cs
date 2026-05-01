using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Zafiro.Avalonia.Controls;
using Zafiro.Avalonia.Controls.Navigation;

namespace Zafiro.Avalonia.Tests;

public class MasterDetailsViewTests
{
    [AvaloniaFact]
    public void SelectedItem_does_not_open_compact_details_by_default()
    {
        var target = Layout(new MasterDetailsView
        {
            CompactWidth = 500,
            SelectedItem = "One",
        }, 300);

        Assert.True(target.IsCompact);
        Assert.False(target.AreDetailsShown);
    }

    [AvaloniaFact]
    public void OpenDetailsCommand_selects_item_and_shows_compact_details()
    {
        var target = Layout(new MasterDetailsView
        {
            CompactWidth = 500,
            ItemsSource = new[] { "One", "Two" },
        }, 300);

        target.OpenDetailsCommand.Execute("Two");

        Assert.Equal("Two", target.SelectedItem);
        Assert.True(target.AreDetailsShown);
    }

    [AvaloniaFact]
    public void OpenDetailsCommand_uses_current_selection_when_parameter_is_null()
    {
        var target = Layout(new MasterDetailsView
        {
            CompactWidth = 500,
            ItemsSource = new[] { "One", "Two" },
            SelectedItem = "One",
        }, 300);

        target.OpenDetailsCommand.Execute(null);

        Assert.Equal("One", target.SelectedItem);
        Assert.True(target.AreDetailsShown);
    }

    [AvaloniaFact]
    public void CloseDetailsCommand_hides_details_without_clearing_selection()
    {
        var target = new MasterDetailsView
        {
            SelectedItem = "One",
            AreDetailsShown = true,
        };

        target.CloseDetailsCommand.Execute(null);

        Assert.Equal("One", target.SelectedItem);
        Assert.False(target.AreDetailsShown);
    }

    [AvaloniaFact]
    public void Clearing_selected_item_hides_details()
    {
        var target = new MasterDetailsView
        {
            SelectedItem = "One",
            AreDetailsShown = true,
        };

        target.SelectedItem = null;

        Assert.False(target.AreDetailsShown);
    }

    [AvaloniaFact]
    public void NavigationKey_change_closes_compact_details()
    {
        var target = new MasterDetailsView
        {
            AreDetailsShown = true,
            NavigationKey = "class-1",
        };

        target.NavigationKey = "class-2";

        Assert.False(target.AreDetailsShown);
    }

    [AvaloniaFact]
    public void Back_participant_is_active_only_when_compact_details_are_shown()
    {
        var target = Layout(new MasterDetailsView
        {
            CompactWidth = 500,
            ItemsSource = new[] { "One" },
            SelectedItem = "One",
        }, 300);

        var participant = Assert.IsAssignableFrom<IFrameBackParticipant>(target);

        Assert.False(participant.CanHandleBack.FirstAsync().Wait());

        target.AreDetailsShown = true;

        Assert.True(participant.CanHandleBack.FirstAsync().Wait());

        target.Measure(new Size(800, 400));
        target.Arrange(new Rect(0, 0, 800, 400));

        Assert.False(participant.CanHandleBack.FirstAsync().Wait());
    }

    [AvaloniaFact]
    public void Switching_to_wide_does_not_clear_selection_or_route_state()
    {
        var target = Layout(new MasterDetailsView
        {
            CompactWidth = 500,
            ItemsSource = new[] { "One" },
            SelectedItem = "One",
            AreDetailsShown = true,
        }, 300);

        target.Measure(new Size(800, 400));
        target.Arrange(new Rect(0, 0, 800, 400));

        Assert.Equal("One", target.SelectedItem);
        Assert.True(target.AreDetailsShown);
        Assert.False(target.IsCompact);
    }

    [AvaloniaFact]
    public void ItemsSource_without_items_sets_empty_state()
    {
        var target = Layout(new MasterDetailsView
        {
            ItemsSource = Array.Empty<string>(),
            EmptyTemplate = new FuncDataTemplate<MasterDetailsViewContext>((_, _) => new TextBlock()),
        }, 300);

        Assert.False(target.HasItems);
        Assert.False(target.HasSelectedItem);
        Assert.True(target.HasEmptyTemplate);
    }

    [AvaloniaFact]
    public void Null_ItemsSource_does_not_clear_preselected_item()
    {
        var target = Layout(new MasterDetailsView
        {
            SelectedItem = "One",
        }, 300);

        Assert.Equal("One", target.SelectedItem);
        Assert.False(target.HasItems);
    }

    [AvaloniaFact]
    public void SelectedItem_removed_from_ItemsSource_closes_details_and_clears_selection()
    {
        var items = new ObservableCollection<string> { "One", "Two" };
        var target = Layout(new MasterDetailsView
        {
            ItemsSource = items,
            SelectedItem = "Two",
            AreDetailsShown = true,
        }, 300);

        items.Remove("Two");

        Assert.Null(target.SelectedItem);
        Assert.False(target.AreDetailsShown);
    }

    [AvaloniaFact]
    public void CompactItemTemplate_overrides_ItemTemplate_for_compact_items()
    {
        var itemTemplate = new FuncDataTemplate<string>((_, _) => new TextBlock());
        var compactItemTemplate = new FuncDataTemplate<string>((_, _) => new TextBlock());
        var target = new MasterDetailsView
        {
            ItemTemplate = itemTemplate,
        };

        Assert.Same(itemTemplate, target.EffectiveCompactItemTemplate);

        target.CompactItemTemplate = compactItemTemplate;

        Assert.Same(compactItemTemplate, target.EffectiveCompactItemTemplate);
    }

    private static MasterDetailsView Layout(MasterDetailsView target, double width)
    {
        var window = new Window
        {
            Content = target,
            Width = width,
            Height = 400,
        };

        window.Show();
        target.Measure(new Size(width, 400));
        target.Arrange(new Rect(0, 0, width, 400));

        return target;
    }
}
