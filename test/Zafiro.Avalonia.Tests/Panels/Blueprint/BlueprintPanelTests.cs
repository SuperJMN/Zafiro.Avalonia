using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Zafiro.Avalonia.Controls.Panels.Blueprint;

namespace Zafiro.Avalonia.Tests.Panels.Blueprint;

public class BlueprintPanelTests
{
    [AvaloniaFact]
    public void Single_layout_arranges_children_in_grid()
    {
        var panel = new BlueprintPanel
        {
            Layout = "0 1 / 2 3",
            Width = 200,
            Height = 100,
        };

        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());

        ShowAndArrange(panel, 200, 100);

        Assert.Equal(new Rect(0, 0, 100, 50), panel.Children[0].Bounds);
        Assert.Equal(new Rect(100, 0, 100, 50), panel.Children[1].Bounds);
        Assert.Equal(new Rect(0, 50, 100, 50), panel.Children[2].Bounds);
        Assert.Equal(new Rect(100, 50, 100, 50), panel.Children[3].Bounds);
    }

    [AvaloniaFact]
    public void Spanning_child_gets_larger_area()
    {
        var panel = new BlueprintPanel
        {
            Layout = "0 0 1\n0 0 1\n2 2 2",
            Width = 300,
            Height = 300,
        };

        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());

        ShowAndArrange(panel, 300, 300);

        Assert.Equal(new Rect(0, 0, 200, 200), panel.Children[0].Bounds);
        Assert.Equal(new Rect(200, 0, 100, 200), panel.Children[1].Bounds);
        Assert.Equal(new Rect(0, 200, 300, 100), panel.Children[2].Bounds);
    }

    [AvaloniaFact]
    public void Spacing_is_applied_between_cells()
    {
        var panel = new BlueprintPanel
        {
            Layout = "0 1\n2 3",
            Width = 210,
            Height = 110,
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());

        ShowAndArrange(panel, 210, 110);

        Assert.Equal(new Rect(0, 0, 100, 50), panel.Children[0].Bounds);
        Assert.Equal(new Rect(110, 0, 100, 50), panel.Children[1].Bounds);
        Assert.Equal(new Rect(0, 60, 100, 50), panel.Children[2].Bounds);
        Assert.Equal(new Rect(110, 60, 100, 50), panel.Children[3].Bounds);
    }

    [AvaloniaFact]
    public void Spacing_within_spanning_child()
    {
        var panel = new BlueprintPanel
        {
            Layout = "0 0\n1 2",
            Width = 210,
            Height = 110,
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());

        ShowAndArrange(panel, 210, 110);

        Assert.Equal(new Rect(0, 0, 210, 50), panel.Children[0].Bounds);
        Assert.Equal(new Rect(0, 60, 100, 50), panel.Children[1].Bounds);
        Assert.Equal(new Rect(110, 60, 100, 50), panel.Children[2].Bounds);
    }

    [AvaloniaFact]
    public void Unreferenced_children_are_collapsed()
    {
        var panel = new BlueprintPanel
        {
            Layout = "0 1",
            Width = 200,
            Height = 100,
        };

        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border()); // Child 2: not in template

        ShowAndArrange(panel, 200, 100);

        Assert.Equal(new Rect(0, 0, 100, 100), panel.Children[0].Bounds);
        Assert.Equal(new Rect(100, 0, 100, 100), panel.Children[1].Bounds);
        Assert.Equal(new Rect(0, 0, 0, 0), panel.Children[2].Bounds);
    }

    [AvaloniaFact]
    public void Responsive_wide_breakpoint_at_1024()
    {
        var panel = CreateResponsivePanel();
        panel.Width = 1200;
        panel.Height = 600;

        ShowAndArrange(panel, 1200, 600);

        // MinWidth=1024 matches: "0 1 2 3" → 1 row, 4 cols
        Assert.Equal(0, panel.Children[0].Bounds.Top);
        Assert.Equal(0, panel.Children[1].Bounds.Top);
        Assert.Equal(0, panel.Children[2].Bounds.Top);
        Assert.Equal(0, panel.Children[3].Bounds.Top);
        // All children in a single row, each 300px wide
        Assert.Equal(300, panel.Children[0].Bounds.Width);
        Assert.Equal(300, panel.Children[1].Bounds.Width);
    }

    [AvaloniaFact]
    public void Responsive_medium_breakpoint_at_800()
    {
        var panel = CreateResponsivePanel();
        panel.Width = 800;
        panel.Height = 600;

        ShowAndArrange(panel, 800, 600);

        // MinWidth=600 matches: "0 1 / 2 3" → 2 rows, 2 cols
        Assert.Equal(new Rect(0, 0, 400, 300), panel.Children[0].Bounds);
        Assert.Equal(new Rect(400, 0, 400, 300), panel.Children[1].Bounds);
        Assert.Equal(new Rect(0, 300, 400, 300), panel.Children[2].Bounds);
        Assert.Equal(new Rect(400, 300, 400, 300), panel.Children[3].Bounds);
    }

    [AvaloniaFact]
    public void Responsive_narrow_fallback_at_400()
    {
        var panel = CreateResponsivePanel();
        panel.Width = 400;
        panel.Height = 800;

        ShowAndArrange(panel, 400, 800);

        // Fallback (no MinWidth): "0 / 1 / 2 / 3" → 4 rows, 1 col
        Assert.Equal(400, panel.Children[0].Bounds.Width);
        Assert.Equal(400, panel.Children[1].Bounds.Width);
        Assert.Equal(0, panel.Children[0].Bounds.Left);
        Assert.Equal(0, panel.Children[1].Bounds.Left);
        // Children stacked vertically
        Assert.True(panel.Children[1].Bounds.Top > panel.Children[0].Bounds.Top);
        Assert.True(panel.Children[2].Bounds.Top > panel.Children[1].Bounds.Top);
        Assert.True(panel.Children[3].Bounds.Top > panel.Children[2].Bounds.Top);
    }

    [AvaloniaFact]
    public void Responsive_switches_layout_when_size_changes()
    {
        var panel = CreateResponsivePanel();

        // Start wide
        panel.Width = 1200;
        panel.Height = 600;
        ShowAndArrange(panel, 1200, 600);

        // Verify wide layout: all children at Y=0
        Assert.Equal(0, panel.Children[0].Bounds.Top);
        Assert.Equal(0, panel.Children[3].Bounds.Top);

        // Shrink to medium
        panel.Width = 700;
        panel.Measure(new Size(700, 600));
        panel.Arrange(new Rect(0, 0, 700, 600));

        // Verify medium layout: 2×2 grid, children 2 and 3 are on row 1
        Assert.True(panel.Children[2].Bounds.Top > 0, "Child 2 should be on second row after resize");
        Assert.True(panel.Children[3].Bounds.Top > 0, "Child 3 should be on second row after resize");
    }

    [AvaloniaFact]
    public void Responsive_children_not_in_narrower_template_are_collapsed()
    {
        var panel = new BlueprintPanel
        {
            Width = 400,
            Height = 400,
        };

        // Wide layout uses child 4; narrow layout doesn't
        panel.Layouts.Add(new LayoutBreakpoint { MinWidth = 800, Blueprint = "0 1\n2 3\n4 4" });
        panel.Layouts.Add(new LayoutBreakpoint { Blueprint = "0\n1\n2\n3" });

        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border()); // Child 4

        ShowAndArrange(panel, 400, 400);

        // Narrow layout active: child 4 is not referenced → collapsed
        Assert.Equal(new Rect(0, 0, 0, 0), panel.Children[4].Bounds);
        // Children 0-3 are arranged
        Assert.True(panel.Children[0].Bounds.Width > 0);
        Assert.True(panel.Children[3].Bounds.Width > 0);
    }

    [AvaloniaFact]
    public void Empty_layout_produces_empty_size()
    {
        var panel = new BlueprintPanel();

        panel.Children.Add(new Border { Width = 50, Height = 50 });

        var window = new Window { Content = panel };
        window.Show();

        panel.Measure(new Size(200, 200));

        Assert.Equal(new Size(0, 0), panel.DesiredSize);
    }

    private static BlueprintPanel CreateResponsivePanel()
    {
        var panel = new BlueprintPanel();

        panel.Layouts.Add(new LayoutBreakpoint { MinWidth = 1024, Blueprint = "0 1 2 3" });
        panel.Layouts.Add(new LayoutBreakpoint { MinWidth = 600, Blueprint = "0 1\n2 3" });
        panel.Layouts.Add(new LayoutBreakpoint { Blueprint = "0\n1\n2\n3" });

        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());

        return panel;
    }

    private static void ShowAndArrange(BlueprintPanel panel, double width, double height)
    {
        var window = new Window { Content = panel };
        window.Show();

        panel.Measure(new Size(width, height));
        panel.Arrange(new Rect(0, 0, width, height));
    }
}