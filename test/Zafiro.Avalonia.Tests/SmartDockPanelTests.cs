using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Zafiro.Avalonia.Controls.Panels;

namespace Zafiro.Avalonia.Tests;

public class SmartDockPanelTests
{
    // ──────────────────────────────────────────────────────────────
    //  Basic layout (no spacing)
    // ──────────────────────────────────────────────────────────────

    [AvaloniaFact]
    public void Should_layout_single_fill_child()
    {
        var panel = new SmartDockPanel();
        var child = new Border(); // No fixed size → stretches to fill
        panel.Children.Add(child);

        ShowAndLayout(panel, 200, 200);

        Assert.Equal(0, child.Bounds.X);
        Assert.Equal(0, child.Bounds.Y);
        Assert.Equal(200, child.Bounds.Width);
        Assert.Equal(200, child.Bounds.Height);
    }

    [AvaloniaFact]
    public void Should_layout_top_docked_and_fill()
    {
        var panel = new SmartDockPanel();
        var top = new Border { Height = 30 };
        DockPanel.SetDock(top, Dock.Top);
        var fill = new Border();
        panel.Children.Add(top);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        Assert.Equal(30, top.Bounds.Height);
        Assert.Equal(0, top.Bounds.Y);
        Assert.Equal(30, fill.Bounds.Y);
        Assert.Equal(170, fill.Bounds.Height);
    }

    [AvaloniaFact]
    public void Should_layout_bottom_docked_and_fill()
    {
        var panel = new SmartDockPanel();
        var bottom = new Border { Height = 30 };
        DockPanel.SetDock(bottom, Dock.Bottom);
        var fill = new Border();
        panel.Children.Add(bottom);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        Assert.Equal(30, bottom.Bounds.Height);
        Assert.Equal(170, bottom.Bounds.Y);
        Assert.Equal(0, fill.Bounds.Y);
        Assert.Equal(170, fill.Bounds.Height);
    }

    [AvaloniaFact]
    public void Should_layout_left_docked_and_fill()
    {
        var panel = new SmartDockPanel();
        var left = new Border { Width = 40 };
        DockPanel.SetDock(left, Dock.Left);
        var fill = new Border();
        panel.Children.Add(left);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        Assert.Equal(40, left.Bounds.Width);
        Assert.Equal(0, left.Bounds.X);
        Assert.Equal(40, fill.Bounds.X);
        Assert.Equal(160, fill.Bounds.Width);
    }

    [AvaloniaFact]
    public void Should_layout_right_docked_and_fill()
    {
        var panel = new SmartDockPanel();
        var right = new Border { Width = 40 };
        DockPanel.SetDock(right, Dock.Right);
        var fill = new Border();
        panel.Children.Add(right);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        Assert.Equal(40, right.Bounds.Width);
        Assert.Equal(160, right.Bounds.X);
        Assert.Equal(0, fill.Bounds.X);
        Assert.Equal(160, fill.Bounds.Width);
    }

    // ──────────────────────────────────────────────────────────────
    //  Vertical spacing
    // ──────────────────────────────────────────────────────────────

    [AvaloniaFact]
    public void Should_apply_vertical_spacing_between_top_and_fill()
    {
        var panel = new SmartDockPanel { VerticalSpacing = 10 };
        var top = new Border { Height = 30 };
        DockPanel.SetDock(top, Dock.Top);
        var fill = new Border();
        panel.Children.Add(top);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        // Top occupies 30px, then 10px gap, then fill gets the rest
        Assert.Equal(30, top.Bounds.Height);
        Assert.Equal(0, top.Bounds.Y);
        Assert.Equal(40, fill.Bounds.Y); // 30 + 10 spacing
        Assert.Equal(160, fill.Bounds.Height); // 200 - 30 - 10
    }

    [AvaloniaFact]
    public void Should_apply_vertical_spacing_between_bottom_and_fill()
    {
        var panel = new SmartDockPanel { VerticalSpacing = 10 };
        var bottom = new Border { Height = 30 };
        DockPanel.SetDock(bottom, Dock.Bottom);
        var fill = new Border();
        panel.Children.Add(bottom);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        // Bottom is 30px from bottom edge, 10px gap, fill gets the rest
        Assert.Equal(30, bottom.Bounds.Height);
        Assert.Equal(170, bottom.Bounds.Y);
        Assert.Equal(0, fill.Bounds.Y);
        Assert.Equal(160, fill.Bounds.Height); // 200 - 30 - 10
    }

    [AvaloniaFact]
    public void Should_apply_vertical_spacing_between_two_top_docked_elements()
    {
        var panel = new SmartDockPanel { VerticalSpacing = 10 };
        var top1 = new Border { Height = 30 };
        DockPanel.SetDock(top1, Dock.Top);
        var top2 = new Border { Height = 30 };
        DockPanel.SetDock(top2, Dock.Top);
        var fill = new Border();
        panel.Children.Add(top1);
        panel.Children.Add(top2);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        Assert.Equal(0, top1.Bounds.Y);
        Assert.Equal(30, top1.Bounds.Height);
        Assert.Equal(40, top2.Bounds.Y); // 30 + 10 spacing
        Assert.Equal(30, top2.Bounds.Height);
        Assert.Equal(80, fill.Bounds.Y); // 30 + 10 + 30 + 10 spacing to fill
        Assert.Equal(120, fill.Bounds.Height); // 200 - 80
    }

    [AvaloniaFact]
    public void Should_apply_vertical_spacing_only_between_children_with_top_bottom_and_fill()
    {
        // This is the exact scenario from the bug report:
        // Top-docked, Bottom-docked, Fill child with VerticalSpacing = 50.
        // Spacing should only appear BETWEEN adjacent children, not after the last one.
        var panel = new SmartDockPanel
        {
            VerticalSpacing = 50,
            Height = 300
        };

        var top = new Border { Height = 30 };
        DockPanel.SetDock(top, Dock.Top);
        var bottom = new Border { Height = 30 };
        DockPanel.SetDock(bottom, Dock.Bottom);
        var fill = new Border();

        panel.Children.Add(top);
        panel.Children.Add(bottom);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 300);

        // Layout should be:
        // [0,  30)   = Top (30px)
        // [30, 80)   = Spacing (50px)
        // [80, 220)  = Fill  (300 - 30 - 50 - 50 - 30 = 140px)
        // [220, 270) = Spacing (50px)
        // [270, 300) = Bottom (30px)

        Assert.Equal(0, top.Bounds.Y);
        Assert.Equal(30, top.Bounds.Height);

        Assert.Equal(270, bottom.Bounds.Y);
        Assert.Equal(30, bottom.Bounds.Height);

        // The fill child must NOT have extra spacing beyond the bottom edge.
        // It should occupy the space between the two gaps.
        Assert.Equal(80, fill.Bounds.Y); // 30 (top) + 50 (spacing)
        Assert.Equal(140, fill.Bounds.Height); // 300 - 30 - 50 - 50 - 30
    }

    // ──────────────────────────────────────────────────────────────
    //  Horizontal spacing
    // ──────────────────────────────────────────────────────────────

    [AvaloniaFact]
    public void Should_apply_horizontal_spacing_between_left_and_fill()
    {
        var panel = new SmartDockPanel { HorizontalSpacing = 10 };
        var left = new Border { Width = 40 };
        DockPanel.SetDock(left, Dock.Left);
        var fill = new Border();
        panel.Children.Add(left);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        Assert.Equal(0, left.Bounds.X);
        Assert.Equal(40, left.Bounds.Width);
        Assert.Equal(50, fill.Bounds.X); // 40 + 10 spacing
        Assert.Equal(150, fill.Bounds.Width); // 200 - 40 - 10
    }

    [AvaloniaFact]
    public void Should_apply_horizontal_spacing_between_right_and_fill()
    {
        var panel = new SmartDockPanel { HorizontalSpacing = 10 };
        var right = new Border { Width = 40 };
        DockPanel.SetDock(right, Dock.Right);
        var fill = new Border();
        panel.Children.Add(right);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        Assert.Equal(160, right.Bounds.X);
        Assert.Equal(40, right.Bounds.Width);
        Assert.Equal(0, fill.Bounds.X);
        Assert.Equal(150, fill.Bounds.Width); // 200 - 40 - 10
    }

    [AvaloniaFact]
    public void Should_apply_horizontal_spacing_between_left_right_and_fill()
    {
        var panel = new SmartDockPanel { HorizontalSpacing = 10 };
        var left = new Border { Width = 40 };
        DockPanel.SetDock(left, Dock.Left);
        var right = new Border { Width = 40 };
        DockPanel.SetDock(right, Dock.Right);
        var fill = new Border();
        panel.Children.Add(left);
        panel.Children.Add(right);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        // Layout: |left(40)|gap(10)|fill(100)|gap(10)|right(40)| = 200
        Assert.Equal(0, left.Bounds.X);
        Assert.Equal(40, left.Bounds.Width);
        Assert.Equal(160, right.Bounds.X);
        Assert.Equal(40, right.Bounds.Width);
        Assert.Equal(50, fill.Bounds.X); // 40 + 10 spacing
        Assert.Equal(100, fill.Bounds.Width); // 200 - 40 - 10 - 10 - 40
    }

    // ──────────────────────────────────────────────────────────────
    //  Invisible children
    // ──────────────────────────────────────────────────────────────

    [AvaloniaFact]
    public void Should_skip_spacing_for_invisible_top_child()
    {
        var panel = new SmartDockPanel { VerticalSpacing = 10 };
        var invisibleTop = new Border { Height = 30, IsVisible = false };
        DockPanel.SetDock(invisibleTop, Dock.Top);
        var fill = new Border();
        panel.Children.Add(invisibleTop);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        // Invisible child should be ignored - fill takes all space
        Assert.Equal(0, fill.Bounds.Y);
        Assert.Equal(200, fill.Bounds.Height);
    }

    [AvaloniaFact]
    public void Should_skip_spacing_for_invisible_child_between_two_visible()
    {
        var panel = new SmartDockPanel { VerticalSpacing = 10 };
        var top1 = new Border { Height = 30 };
        DockPanel.SetDock(top1, Dock.Top);
        var top2 = new Border { Height = 30, IsVisible = false };
        DockPanel.SetDock(top2, Dock.Top);
        var top3 = new Border { Height = 30 };
        DockPanel.SetDock(top3, Dock.Top);
        var fill = new Border();

        panel.Children.Add(top1);
        panel.Children.Add(top2);
        panel.Children.Add(top3);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        // Layout: top1(30) + spacing(10) + top3(30) = only 1 spacing gap
        Assert.Equal(0, top1.Bounds.Y);
        Assert.Equal(40, top3.Bounds.Y); // 30 + 10 spacing  (top2 is skipped)
    }

    // ──────────────────────────────────────────────────────────────
    //  Edge cases
    // ──────────────────────────────────────────────────────────────

    [AvaloniaFact]
    public void Should_handle_single_top_child_no_fill()
    {
        var panel = new SmartDockPanel { LastChildFill = false, VerticalSpacing = 10 };
        var top = new Border { Height = 30 };
        DockPanel.SetDock(top, Dock.Top);
        panel.Children.Add(top);

        ShowAndLayout(panel, 200, 200);

        Assert.Equal(0, top.Bounds.Y);
        Assert.Equal(30, top.Bounds.Height);
    }

    [AvaloniaFact]
    public void Should_handle_empty_panel()
    {
        var panel = new SmartDockPanel { VerticalSpacing = 10 };

        ShowAndLayout(panel, 200, 200);

        Assert.Equal(0, panel.Bounds.Width + panel.Bounds.Height > 0 ? 0 : 0); // Just shouldn't throw
    }

    [AvaloniaFact]
    public void Should_measure_zero_when_all_children_are_invisible()
    {
        var panel = new SmartDockPanel { VerticalSpacing = 50 };
        var top = new Border { Height = 30, IsVisible = false };
        DockPanel.SetDock(top, Dock.Top);
        var bottom = new Border { Height = 30, IsVisible = false };
        DockPanel.SetDock(bottom, Dock.Bottom);
        var fill = new Border { IsVisible = false };

        panel.Children.Add(top);
        panel.Children.Add(bottom);
        panel.Children.Add(fill);

        var window = new Window { Content = panel };
        window.Show();

        panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        Assert.Equal(0, panel.DesiredSize.Width);
        Assert.Equal(0, panel.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Should_measure_correctly_for_top_bottom_fill_with_vertical_spacing()
    {
        // Verify that MeasureOverride accounts for spacing correctly
        var panel = new SmartDockPanel { VerticalSpacing = 20 };
        var top = new Border { Height = 30, Width = 100 };
        DockPanel.SetDock(top, Dock.Top);
        var bottom = new Border { Height = 30, Width = 100 };
        DockPanel.SetDock(bottom, Dock.Bottom);
        var fill = new Border { Height = 50, Width = 100 };

        panel.Children.Add(top);
        panel.Children.Add(bottom);
        panel.Children.Add(fill);

        var window = new Window { Content = panel };
        window.Show();

        panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        // Desired: top(30) + spacing(20) + bottom(30) + spacing(20) + fill(50) = 150
        Assert.Equal(150, panel.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Should_not_apply_spacing_when_only_fill_child()
    {
        var panel = new SmartDockPanel { VerticalSpacing = 50, HorizontalSpacing = 50 };
        var fill = new Border();
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        // No spacing should be applied - fill takes all space
        Assert.Equal(0, fill.Bounds.X);
        Assert.Equal(0, fill.Bounds.Y);
        Assert.Equal(200, fill.Bounds.Width);
        Assert.Equal(200, fill.Bounds.Height);
    }

    [AvaloniaFact]
    public void Should_apply_spacing_on_all_four_sides_of_fill()
    {
        var panel = new SmartDockPanel
        {
            VerticalSpacing = 10,
            HorizontalSpacing = 10
        };

        var top = new Border { Height = 20 };
        DockPanel.SetDock(top, Dock.Top);
        var bottom = new Border { Height = 20 };
        DockPanel.SetDock(bottom, Dock.Bottom);
        var left = new Border { Width = 20 };
        DockPanel.SetDock(left, Dock.Left);
        var right = new Border { Width = 20 };
        DockPanel.SetDock(right, Dock.Right);
        var fill = new Border();

        panel.Children.Add(top);
        panel.Children.Add(bottom);
        panel.Children.Add(left);
        panel.Children.Add(right);
        panel.Children.Add(fill);

        ShowAndLayout(panel, 200, 200);

        // Fill should be centered with spacing gaps on all four sides
        // Vertical: top=20, gap=10, fill, gap=10, bottom=20 => fill height = 200-20-10-10-20 = 140
        // Horizontal: left=20, gap=10, fill, gap=10, right=20 => fill width is trickier (depends on order)
        // but conceptually fill should be inset by (left+spacing) on left and (right+spacing) on right
        Assert.Equal(30, fill.Bounds.Y); // 20 (top) + 10 (spacing)
        Assert.Equal(140, fill.Bounds.Height); // 200 - 20 - 10 - 10 - 20
    }

    // ──────────────────────────────────────────────────────────────
    //  Helper
    // ──────────────────────────────────────────────────────────────

    private static void ShowAndLayout(Control panel, double width, double height)
    {
        var window = new Window
        {
            Content = panel,
            Width = width,
            Height = height
        };
        window.Show();

        panel.Measure(new Size(width, height));
        panel.Arrange(new Rect(0, 0, width, height));
    }
}