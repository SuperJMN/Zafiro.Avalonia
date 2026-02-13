using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Zafiro.Avalonia.Controls.Panels;

namespace Zafiro.Avalonia.Tests;

public class SmartGridTests
{
    [AvaloniaFact]
    public void Should_measure_implicit_auto_track_when_no_definitions_are_declared()
    {
        var target = new SmartGrid
        {
            Children =
            {
                new Border { Width = 50, Height = 40 }
            }
        };

        var window = new Window { Content = target };
        window.Show();

        target.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        target.Arrange(new Rect(target.DesiredSize));

        Assert.Equal(50, target.DesiredSize.Width);
        Assert.Equal(40, target.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Should_collapse_invisible_pixel_tracks()
    {
        var target = new SmartGrid
        {
            RowDefinitions = RowDefinitions.Parse("10,10,10"),
            RowSpacing = 5,
            Children =
            {
                new Border { [SmartGrid.RowProperty] = 0 },
                new Border { [SmartGrid.RowProperty] = 1, IsVisible = false },
                new Border { [SmartGrid.RowProperty] = 2 }
            }
        };

        var window = new Window { Content = target };
        window.Show();

        target.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        Assert.Equal(25, target.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Should_layout_visible_items_when_only_column_definitions_are_declared()
    {
        var target = new SmartGrid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("Auto,Auto"),
            ColumnSpacing = 20,
            Children =
            {
                new Border { Width = 30, Height = 10 },
                new Border { Width = 40, Height = 10, [SmartGrid.ColumnProperty] = 1 }
            }
        };

        var window = new Window { Content = target };
        window.Show();

        target.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        target.Arrange(new Rect(target.DesiredSize));

        Assert.Equal(90, target.DesiredSize.Width);
        Assert.Equal(10, target.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Should_collapse_spacing_around_invisible_rows()
    {
        var target = new SmartGrid
        {
            RowDefinitions = RowDefinitions.Parse("Auto,Auto,Auto"),
            RowSpacing = 10,
            Children =
            {
                new Border { Height = 50, [SmartGrid.RowProperty] = 0 },
                new Border { Height = 50, [SmartGrid.RowProperty] = 1, IsVisible = false },
                new Border { Height = 50, [SmartGrid.RowProperty] = 2 },
            }
        };

        var window = new Window
        {
            Content = target,
            Width = 200,
            Height = 500, // Sobrado para que no constraint
        };

        window.Show();

        // Forzamos layout
        target.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        target.Arrange(new Rect(target.DesiredSize));

        // Esperamos: 50 (Row0) + 10 (Spacing) + 50 (Row2) = 110
        // Si fallara como un Grid normal con filas vacías, podría ser 120 (doble spacing)
        Assert.Equal(110, target.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Should_handle_star_sizing_correctly()
    {
        var target = new SmartGrid
        {
            RowDefinitions = RowDefinitions.Parse("*,*"),
            RowSpacing = 10,
            Height = 110, // 50 + 10 + 50
            Children =
            {
                new Border { [SmartGrid.RowProperty] = 0 },
                new Border { [SmartGrid.RowProperty] = 1 },
            }
        };

        var window = new Window { Content = target };
        window.Show();

        target.Measure(new Size(100, 110));
        target.Arrange(new Rect(0, 0, 100, 110));

        var child1 = (Control)target.Children[0];
        var child2 = (Control)target.Children[1];

        Assert.Equal(50, child1.Bounds.Height);
        Assert.Equal(50, child2.Bounds.Height);
    }

    [AvaloniaFact]
    public void Should_compress_gaps_correctly_with_mixed_visibility()
    {
        // 4 Rows. Row 2 is hidden.
        // Expected: Row0 + Gap + Row1 + Gap + Row3.
        // Total Gaps: 2.

        var target = new SmartGrid
        {
            RowDefinitions = RowDefinitions.Parse("Auto,Auto,Auto,Auto"),
            RowSpacing = 10,
            Children =
            {
                new Border { Height = 10, [SmartGrid.RowProperty] = 0 }, // Visible
                new Border { Height = 10, [SmartGrid.RowProperty] = 1 }, // Visible
                new Border { Height = 10, [SmartGrid.RowProperty] = 2, IsVisible = false }, // Hidden
                new Border { Height = 10, [SmartGrid.RowProperty] = 3 }, // Visible
            }
        };

        var window = new Window { Content = target };
        window.Show();

        target.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        // Height = 10*3 + 10*2 = 30 + 20 = 50.
        // Standard Grid would be 30 + 30 = 60.
        Assert.Equal(50, target.DesiredSize.Height);
    }
}