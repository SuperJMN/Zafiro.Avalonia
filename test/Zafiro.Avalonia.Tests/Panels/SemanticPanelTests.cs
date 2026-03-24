using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Zafiro.Avalonia.Controls.Panels;

namespace Zafiro.Avalonia.Tests.Panels;

public class SemanticPanelTests
{
    // ── Helpers ──────────────────────────────────────────────────────

    private static Control Item(double width, double height, SemanticRole role)
    {
        var control = new FixedSizeControl(width, height);
        SemanticPanel.SetRole(control, role);
        return control;
    }

    private static void Layout(Panel panel, double width, double height)
    {
        var window = new Window { Content = panel };
        window.Show();
        panel.Measure(new Size(width, height));
        panel.Arrange(new Rect(0, 0, width, height));
    }

    private static void AssertBounds(Control control, double x, double y, double width, double height,
        int precision = 0)
    {
        Assert.Equal(x, control.Bounds.X, precision);
        Assert.Equal(y, control.Bounds.Y, precision);
        Assert.Equal(width, control.Bounds.Width, precision);
        Assert.Equal(height, control.Bounds.Height, precision);
    }

    // ── Compact layout tests (< 600px) ──────────────────────────────

    [AvaloniaFact]
    public void Compact_stacks_all_roles_vertically_in_priority_order()
    {
        var primary = Item(400, 200, SemanticRole.Primary);
        var actionPrimary = Item(400, 40, SemanticRole.ActionPrimary);
        var info = Item(400, 60, SemanticRole.Info);
        var secondary = Item(400, 100, SemanticRole.Secondary);
        var actionSecondary = Item(400, 30, SemanticRole.ActionSecondary);
        var sidebar = Item(400, 50, SemanticRole.Sidebar);

        var panel = new SemanticPanel
        {
            Spacing = 0,
            Children = { primary, secondary, info, actionPrimary, actionSecondary, sidebar },
        };

        Layout(panel, 400, 800);

        Assert.Equal(SizeClass.Compact, panel.CurrentSizeClass);

        // Compact order: Primary, ActionPrimary, Info, Secondary, ActionSecondary, Sidebar
        AssertBounds(primary, 0, 0, 400, 200);
        AssertBounds(actionPrimary, 0, 200, 400, 40);
        AssertBounds(info, 0, 240, 400, 60);
        AssertBounds(secondary, 0, 300, 400, 100);
        AssertBounds(actionSecondary, 0, 400, 400, 30);
        AssertBounds(sidebar, 0, 430, 400, 50);
    }

    [AvaloniaFact]
    public void Compact_with_spacing_adds_gaps_between_roles()
    {
        var primary = Item(400, 200, SemanticRole.Primary);
        var info = Item(400, 60, SemanticRole.Info);

        var panel = new SemanticPanel
        {
            Spacing = 10,
            Children = { primary, info },
        };

        Layout(panel, 400, 800);

        AssertBounds(primary, 0, 0, 400, 200);
        AssertBounds(info, 0, 210, 400, 60);
    }

    [AvaloniaFact]
    public void Compact_with_only_primary_takes_full_width()
    {
        var primary = Item(400, 300, SemanticRole.Primary);
        var panel = new SemanticPanel { Spacing = 0, Children = { primary } };
        Layout(panel, 400, 800);

        AssertBounds(primary, 0, 0, 400, 300);
    }

    // ── Expanded layout tests (≥ 900px) ─────────────────────────────

    [AvaloniaFact]
    public void Expanded_places_primary_center_secondary_right()
    {
        var primary = Item(500, 300, SemanticRole.Primary);
        var secondary = Item(200, 300, SemanticRole.Secondary);

        var panel = new SemanticPanel
        {
            Spacing = 0,
            PrimaryRatio = 0.6,
            Children = { primary, secondary },
        };

        Layout(panel, 1000, 600);

        Assert.Equal(SizeClass.Expanded, panel.CurrentSizeClass);

        // No sidebar → Primary starts at x=0, width = 1000 * 0.6 = 600
        // Secondary at x=600, width = 1000 * 0.4 = 400
        AssertBounds(primary, 0, 0, 600, 300);
        AssertBounds(secondary, 600, 0, 400, 300);
    }

    [AvaloniaFact]
    public void Expanded_with_sidebar_shifts_content_right()
    {
        var sidebar = Item(250, 600, SemanticRole.Sidebar);
        var primary = Item(400, 300, SemanticRole.Primary);
        var secondary = Item(200, 300, SemanticRole.Secondary);

        var panel = new SemanticPanel
        {
            Spacing = 10,
            SidebarWidth = 250,
            PrimaryRatio = 0.65,
            Children = { sidebar, primary, secondary },
        };

        Layout(panel, 1000, 600);

        Assert.Equal(SizeClass.Expanded, panel.CurrentSizeClass);

        // Sidebar at x=0, width=250
        Assert.Equal(0d, sidebar.Bounds.X, 0);
        Assert.Equal(250d, sidebar.Bounds.Width, 0);

        // Content starts at 250+10 = 260. Content width = 1000 - 260 = 740
        Assert.Equal(260d, primary.Bounds.X, 0);
        Assert.True(primary.Bounds.Width > 0);

        // Secondary is to the RIGHT of primary
        Assert.True(secondary.Bounds.X > primary.Bounds.X);
    }

    [AvaloniaFact]
    public void Expanded_info_below_primary_same_width()
    {
        var primary = Item(600, 300, SemanticRole.Primary);
        var info = Item(600, 80, SemanticRole.Info);
        var secondary = Item(200, 400, SemanticRole.Secondary);

        var panel = new SemanticPanel
        {
            Spacing = 10,
            PrimaryRatio = 0.65,
            Children = { primary, info, secondary },
        };

        Layout(panel, 1000, 600);

        // Info is below Primary, same X and width
        Assert.Equal(primary.Bounds.X, info.Bounds.X, 0);
        Assert.Equal(primary.Bounds.Width, info.Bounds.Width, 0);
        Assert.Equal(primary.Bounds.Bottom + 10, info.Bounds.Y, 0);
    }

    [AvaloniaFact]
    public void Expanded_actions_below_content_area()
    {
        var primary = Item(600, 200, SemanticRole.Primary);
        var actionPrimary = Item(600, 40, SemanticRole.ActionPrimary);

        var panel = new SemanticPanel
        {
            Spacing = 10,
            Children = { primary, actionPrimary },
        };

        Layout(panel, 1000, 600);

        // ActionPrimary below Primary
        Assert.True(actionPrimary.Bounds.Y >= primary.Bounds.Bottom);
    }

    [AvaloniaFact]
    public void Expanded_without_secondary_primary_takes_full_content_width()
    {
        var primary = Item(800, 300, SemanticRole.Primary);

        var panel = new SemanticPanel
        {
            Spacing = 0,
            SidebarWidth = 200,
            Children = { primary },
        };

        Layout(panel, 1000, 600);

        // No sidebar child → no sidebar column → primary = full width
        Assert.Equal(1000d, primary.Bounds.Width, 0);
    }

    // ── Medium layout tests (600–899px) ─────────────────────────────

    [AvaloniaFact]
    public void Medium_side_by_side_when_secondary_fits()
    {
        var primary = Item(400, 200, SemanticRole.Primary);
        var secondary = Item(200, 200, SemanticRole.Secondary);

        var panel = new SemanticPanel
        {
            Spacing = 0,
            PrimaryRatio = 0.6,
            SecondaryMinWidth = 200,
            Children = { primary, secondary },
        };

        Layout(panel, 700, 600);

        Assert.Equal(SizeClass.Medium, panel.CurrentSizeClass);

        // Side by side: Primary left, Secondary right
        Assert.Equal(0d, primary.Bounds.X, 0);
        Assert.True(secondary.Bounds.X > 0);
        Assert.Equal(0d, secondary.Bounds.Y, 0);
    }

    [AvaloniaFact]
    public void Medium_sidebar_goes_to_bottom()
    {
        var primary = Item(600, 200, SemanticRole.Primary);
        var sidebar = Item(600, 50, SemanticRole.Sidebar);

        var panel = new SemanticPanel
        {
            Spacing = 10,
            Children = { primary, sidebar },
        };

        Layout(panel, 700, 600);

        Assert.Equal(SizeClass.Medium, panel.CurrentSizeClass);

        // Sidebar is below Primary on Medium
        Assert.True(sidebar.Bounds.Y > primary.Bounds.Bottom);
        Assert.Equal(700d, sidebar.Bounds.Width, 0);
    }

    // ── Missing roles ───────────────────────────────────────────────

    [AvaloniaFact]
    public void Works_with_only_primary_and_actions()
    {
        var primary = Item(400, 200, SemanticRole.Primary);
        var actionPrimary = Item(400, 40, SemanticRole.ActionPrimary);

        var panel = new SemanticPanel
        {
            Spacing = 10,
            Children = { primary, actionPrimary },
        };

        Layout(panel, 1000, 600);

        Assert.Equal(SizeClass.Expanded, panel.CurrentSizeClass);
        Assert.True(primary.Bounds.Width > 0);
        Assert.True(actionPrimary.Bounds.Width > 0);
        Assert.True(actionPrimary.Bounds.Y > primary.Bounds.Y);
    }

    [AvaloniaFact]
    public void Empty_panel_does_not_crash()
    {
        var panel = new SemanticPanel();
        Layout(panel, 1000, 600);
    }

    [AvaloniaFact]
    public void Invisible_children_are_excluded()
    {
        var primary = Item(400, 200, SemanticRole.Primary);
        var hidden = Item(400, 200, SemanticRole.Secondary);
        hidden.IsVisible = false;

        var panel = new SemanticPanel
        {
            Spacing = 0,
            Children = { primary, hidden },
        };

        Layout(panel, 1000, 600);

        // Primary takes full width because Secondary is invisible
        Assert.Equal(1000d, primary.Bounds.Width, 0);
    }

    // ── SizeClass transitions ───────────────────────────────────────

    [AvaloniaFact]
    public void SizeClass_is_compact_below_breakpoint()
    {
        var primary = Item(400, 200, SemanticRole.Primary);
        var panel = new SemanticPanel
        {
            CompactBreakpoint = 600,
            ExpandedBreakpoint = 900,
            Children = { primary },
        };

        Layout(panel, 500, 600);
        Assert.Equal(SizeClass.Compact, panel.CurrentSizeClass);
    }

    [AvaloniaFact]
    public void SizeClass_is_medium_between_breakpoints()
    {
        var primary = Item(400, 200, SemanticRole.Primary);
        var panel = new SemanticPanel
        {
            CompactBreakpoint = 600,
            ExpandedBreakpoint = 900,
            Children = { primary },
        };

        Layout(panel, 750, 600);
        Assert.Equal(SizeClass.Medium, panel.CurrentSizeClass);
    }

    [AvaloniaFact]
    public void SizeClass_is_expanded_above_breakpoint()
    {
        var primary = Item(400, 200, SemanticRole.Primary);
        var panel = new SemanticPanel
        {
            CompactBreakpoint = 600,
            ExpandedBreakpoint = 900,
            Children = { primary },
        };

        Layout(panel, 1000, 600);
        Assert.Equal(SizeClass.Expanded, panel.CurrentSizeClass);
    }

    // ── Medium stacked: ActionSecondary alignment ─────────────────

    [AvaloniaFact]
    public void Medium_stacked_ActionSecondary_aligned_with_ActionPrimary()
    {
        // Reproduces the TV Broadcast scenario at 500px:
        // CompactBreakpoint=500 → classified as Medium
        // 500 * 0.35 = 175 < SecondaryMinWidth(200) → stacked layout
        var primary = Item(500, 220, SemanticRole.Primary);
        var secondary = Item(500, 161, SemanticRole.Secondary);
        var info = Item(500, 63, SemanticRole.Info);
        var actionPrimary = Item(500, 63, SemanticRole.ActionPrimary);
        var actionSecondary = Item(500, 63, SemanticRole.ActionSecondary);
        var sidebar = Item(500, 166, SemanticRole.Sidebar);

        var panel = new SemanticPanel
        {
            Spacing = 8,
            CompactBreakpoint = 500,
            ExpandedBreakpoint = 800,
            PrimaryRatio = 0.65,
            SecondaryMinWidth = 200,
            SidebarWidth = 180,
            Children = { primary, secondary, info, actionPrimary, actionSecondary, sidebar },
        };

        Layout(panel, 500, 900);

        Assert.Equal(SizeClass.Medium, panel.CurrentSizeClass);

        // ActionPrimary and ActionSecondary must share the same Y (same row)
        Assert.Equal(actionPrimary.Bounds.Y, actionSecondary.Bounds.Y, 0);

        // Sidebar must be exactly one spacing below the actions row
        var actionsBottom = Math.Max(actionPrimary.Bounds.Bottom, actionSecondary.Bounds.Bottom);
        Assert.Equal(actionsBottom + 8, sidebar.Bounds.Y, 0);
    }

    // ── Desired size ────────────────────────────────────────────────

    [AvaloniaFact]
    public void Compact_desired_height_is_sum_of_children_plus_spacing()
    {
        var primary = Item(400, 200, SemanticRole.Primary);
        var info = Item(400, 60, SemanticRole.Info);
        var actionPrimary = Item(400, 40, SemanticRole.ActionPrimary);

        var panel = new SemanticPanel
        {
            Spacing = 10,
            Children = { primary, info, actionPrimary },
        };

        Layout(panel, 400, 800);

        // Order: Primary(200) + 10 + ActionPrimary(40) + 10 + Info(60) = 320
        Assert.Equal(320d, panel.DesiredSize.Height, 0);
    }

    // ── FixedSizeControl ────────────────────────────────────────────

    private sealed class FixedSizeControl(double width, double height) : Control
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(
                Math.Min(width, availableSize.Width),
                height);
        }
    }
}