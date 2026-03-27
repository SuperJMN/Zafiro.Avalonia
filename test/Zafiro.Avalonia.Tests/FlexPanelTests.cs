using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Zafiro.Avalonia.Controls.Panels;

namespace Zafiro.Avalonia.Tests;

public class FlexPanelTests
{
    [AvaloniaTheory]
    [InlineData(FlexJustify.Start, 0, 30, 70)]
    [InlineData(FlexJustify.End, 80, 110, 150)]
    [InlineData(FlexJustify.Center, 40, 70, 110)]
    [InlineData(FlexJustify.SpaceBetween, 0, 70, 150)]
    [InlineData(FlexJustify.SpaceAround, 13, 70, 137)]
    [InlineData(FlexJustify.SpaceEvenly, 20, 70, 130)]
    public void Should_apply_justify_content_on_a_single_row_line(FlexJustify justifyContent, double firstX, double secondX, double thirdX)
    {
        var first = Item(width: 30, height: 10);
        var second = Item(width: 40, height: 10);
        var third = Item(width: 50, height: 10);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, justifyContent, FlexAlign.Start, 0, first, second, third);

        ShowAndLayout(panel, 200, 100);

        AssertBounds(first, x: firstX, y: 0, width: 30, height: 10);
        AssertBounds(second, x: secondX, y: 0, width: 40, height: 10);
        AssertBounds(third, x: thirdX, y: 0, width: 50, height: 10);
    }

    [AvaloniaTheory]
    [InlineData(FlexJustify.Start, 0)]
    [InlineData(FlexJustify.End, 170)]
    [InlineData(FlexJustify.Center, 85)]
    [InlineData(FlexJustify.SpaceBetween, 0)]
    [InlineData(FlexJustify.SpaceAround, 85)]
    [InlineData(FlexJustify.SpaceEvenly, 85)]
    public void Should_match_css_single_item_behavior_for_justify_content(FlexJustify justifyContent, double expectedX)
    {
        var item = Item(width: 30, height: 10);
        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, justifyContent, FlexAlign.Start, 0, item);

        ShowAndLayout(panel, 200, 100);

        AssertBounds(item, x: expectedX, y: 0, width: 30, height: 10);
    }

    [AvaloniaFact]
    public void Should_layout_row_children_left_to_right()
    {
        var first = Item(width: 30, height: 10);
        var second = Item(width: 40, height: 10);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second);

        ShowAndLayout(panel, 200, 100);

        AssertBounds(first, x: 0, y: 0, width: 30, height: 10);
        AssertBounds(second, x: 30, y: 0, width: 40, height: 10);
    }

    [AvaloniaFact]
    public void Should_layout_row_reverse_from_right_edge()
    {
        var first = Item(width: 30, height: 10);
        var second = Item(width: 40, height: 10);

        var panel = Panel(FlexDirection.RowReverse, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second);

        ShowAndLayout(panel, 120, 100);

        AssertBounds(first, x: 90, y: 0, width: 30, height: 10);
        AssertBounds(second, x: 50, y: 0, width: 40, height: 10);
    }

    [AvaloniaFact]
    public void Should_layout_column_reverse_from_bottom_edge()
    {
        var first = Item(width: 20, height: 30);
        var second = Item(width: 20, height: 40);

        var panel = Panel(FlexDirection.ColumnReverse, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second);

        ShowAndLayout(panel, 100, 120);

        AssertBounds(first, x: 0, y: 90, width: 20, height: 30);
        AssertBounds(second, x: 0, y: 50, width: 20, height: 40);
    }

    [AvaloniaFact]
    public void Should_honor_order_before_layout()
    {
        var first = Item(width: 20, height: 10);
        var second = Item(width: 30, height: 10);
        var third = Item(width: 40, height: 10);

        FlexPanel.SetOrder(first, 2);
        FlexPanel.SetOrder(second, 0);
        FlexPanel.SetOrder(third, 1);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second, third);

        ShowAndLayout(panel, 200, 100);

        Assert.Equal(70, first.Bounds.X);
        Assert.Equal(0, second.Bounds.X);
        Assert.Equal(30, third.Bounds.X);
    }

    [AvaloniaFact]
    public void Should_apply_gap_on_main_axis()
    {
        var first = Item(width: 30, height: 10);
        var second = Item(width: 40, height: 10);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 10, first, second);

        ShowAndLayout(panel, 200, 100);

        AssertBounds(first, x: 0, y: 0, width: 30, height: 10);
        AssertBounds(second, x: 40, y: 0, width: 40, height: 10);
    }

    [AvaloniaFact]
    public void Should_distribute_positive_free_space_using_grow_factor()
    {
        var first = Item(width: 50, height: 10);
        var second = Item(width: 50, height: 10);
        var third = Item(width: 50, height: 10);

        FlexPanel.SetGrow(second, 1);
        FlexPanel.SetGrow(third, 2);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second, third);

        ShowAndLayout(panel, 300, 100);

        AssertBounds(first, x: 0, y: 0, width: 50, height: 10);
        AssertBounds(second, x: 50, y: 0, width: 100, height: 10);
        AssertBounds(third, x: 150, y: 0, width: 150, height: 10);
    }

    [AvaloniaFact]
    public void Should_distribute_negative_free_space_using_shrink_factor()
    {
        var first = Item(width: 100, height: 10);
        var second = Item(width: 100, height: 10);

        FlexPanel.SetShrink(first, 1);
        FlexPanel.SetShrink(second, 1);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second);

        ShowAndLayout(panel, 150, 100);

        AssertBounds(first, x: 0, y: 0, width: 75, height: 10);
        AssertBounds(second, x: 75, y: 0, width: 75, height: 10);
    }

    [AvaloniaFact]
    public void Should_support_common_equal_width_web_pattern_using_flex_basis_zero_and_grow()
    {
        var first = Item(width: 80, height: 10);
        var second = Item(width: 20, height: 10);
        var third = Item(width: 50, height: 10);

        FlexPanel.SetFlex(first, FlexValue.Create(grow: 1, shrink: 1, basis: FlexBasis.Pixels(0)));
        FlexPanel.SetFlex(second, FlexValue.Create(grow: 1, shrink: 1, basis: FlexBasis.Pixels(0)));
        FlexPanel.SetFlex(third, FlexValue.Create(grow: 1, shrink: 1, basis: FlexBasis.Pixels(0)));

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second, third);

        ShowAndLayout(panel, 300, 100);

        AssertBounds(first, x: 0, y: 0, width: 100, height: 10);
        AssertBounds(second, x: 100, y: 0, width: 100, height: 10);
        AssertBounds(third, x: 200, y: 0, width: 100, height: 10);
    }

    [AvaloniaFact]
    public void Should_wrap_items_onto_new_lines()
    {
        var first = Item(width: 40, height: 20);
        var second = Item(width: 40, height: 20);
        var third = Item(width: 40, height: 20);

        var panel = Panel(FlexDirection.Row, FlexWrap.Wrap, FlexJustify.Start, FlexAlign.Start, 10, first, second, third);
        panel.AlignContent = FlexAlignContent.Start;

        ShowAndLayout(panel, 100, 100);

        AssertBounds(first, x: 0, y: 0, width: 40, height: 20);
        AssertBounds(second, x: 50, y: 0, width: 40, height: 20);
        AssertBounds(third, x: 0, y: 30, width: 40, height: 20);
    }

    [AvaloniaFact]
    public void Should_apply_justify_content_independently_on_each_wrapped_line_in_common_navigation_layout()
    {
        var first = Item(width: 40, height: 20);
        var second = Item(width: 40, height: 20);
        var third = Item(width: 40, height: 20);
        var fourth = Item(width: 40, height: 20);
        var fifth = Item(width: 40, height: 20);

        var panel = Panel(FlexDirection.Row, FlexWrap.Wrap, FlexJustify.SpaceBetween, FlexAlign.Center, 12, first, second, third, fourth, fifth);
        panel.AlignContent = FlexAlignContent.Start;

        ShowAndLayout(panel, 120, 200);

        AssertBounds(first, x: 0, y: 0, width: 40, height: 20);
        AssertBounds(second, x: 80, y: 0, width: 40, height: 20);
        AssertBounds(third, x: 0, y: 32, width: 40, height: 20);
        AssertBounds(fourth, x: 80, y: 32, width: 40, height: 20);
        AssertBounds(fifth, x: 0, y: 64, width: 40, height: 20);
    }

    [AvaloniaFact]
    public void Should_stretch_lines_on_cross_axis_by_default_when_wrapping()
    {
        var first = Item(width: 40, height: 20);
        var second = Item(width: 40, height: 20);
        var third = Item(width: 40, height: 20);

        var panel = Panel(FlexDirection.Row, FlexWrap.Wrap, FlexJustify.Start, FlexAlign.Start, 10, first, second, third);

        ShowAndLayout(panel, 100, 100);

        AssertBounds(first, x: 0, y: 0, width: 40, height: 20);
        AssertBounds(second, x: 50, y: 0, width: 40, height: 20);
        AssertBounds(third, x: 0, y: 55, width: 40, height: 20);
    }

    [AvaloniaTheory]
    [InlineData(FlexAlign.Start, 0, 0, 20, 40)]
    [InlineData(FlexAlign.End, 80, 60, 20, 40)]
    [InlineData(FlexAlign.Center, 40, 30, 20, 40)]
    [InlineData(FlexAlign.Stretch, 0, 0, 100, 100)]
    public void Should_apply_align_items_on_the_cross_axis(FlexAlign alignItems, double firstY, double secondY, double firstHeight, double secondHeight)
    {
        var first = Item(width: 30, height: 20);
        var second = Item(width: 40, height: 40);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, alignItems, 0, first, second);

        ShowAndLayout(panel, 200, 100);

        AssertBounds(first, x: 0, y: firstY, width: 30, height: firstHeight);
        AssertBounds(second, x: 30, y: secondY, width: 40, height: secondHeight);
    }

    [AvaloniaFact]
    public void Should_not_stretch_items_with_explicit_cross_size()
    {
        var first = new Border { Width = 30, Height = 20 };
        var second = new Border { Width = 40, Height = 40 };

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Stretch, 0, first, second);

        ShowAndLayout(panel, 200, 100);

        AssertBounds(first, x: 0, y: 0, width: 30, height: 20);
        AssertBounds(second, x: 30, y: 0, width: 40, height: 40);
    }

    [AvaloniaFact]
    public void Should_allow_align_self_to_override_align_items()
    {
        var first = Item(width: 30, height: 20);
        var second = Item(width: 40, height: 40);

        FlexPanel.SetAlignSelf(second, FlexAlign.Center);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.End, 0, first, second);

        ShowAndLayout(panel, 200, 100);

        AssertBounds(first, x: 0, y: 80, width: 30, height: 20);
        AssertBounds(second, x: 30, y: 30, width: 40, height: 40);
    }

    [AvaloniaTheory]
    [InlineData(FlexAlignContent.Start, 0, 30)]
    [InlineData(FlexAlignContent.End, 50, 80)]
    [InlineData(FlexAlignContent.Center, 25, 55)]
    [InlineData(FlexAlignContent.SpaceBetween, 0, 80)]
    [InlineData(FlexAlignContent.SpaceAround, 12, 68)]
    [InlineData(FlexAlignContent.SpaceEvenly, 17, 63)]
    [InlineData(FlexAlignContent.Stretch, 0, 55)]
    public void Should_apply_align_content_for_wrapped_lines(FlexAlignContent alignContent, double firstLineY, double secondLineY)
    {
        var first = Item(width: 40, height: 20);
        var second = Item(width: 40, height: 20);
        var third = Item(width: 40, height: 20);

        var panel = Panel(FlexDirection.Row, FlexWrap.Wrap, FlexJustify.Start, FlexAlign.Start, 10, first, second, third);
        panel.AlignContent = alignContent;

        ShowAndLayout(panel, 100, 100);

        AssertBounds(first, x: 0, y: firstLineY, width: 40, height: 20);
        AssertBounds(second, x: 50, y: firstLineY, width: 40, height: 20);
        AssertBounds(third, x: 0, y: secondLineY, width: 40, height: 20);
    }

    [AvaloniaFact]
    public void Should_apply_align_content_even_when_wrap_is_enabled_but_only_one_line_is_needed()
    {
        var first = Item(width: 30, height: 20);
        var second = Item(width: 40, height: 20);

        var panel = Panel(FlexDirection.Row, FlexWrap.Wrap, FlexJustify.Start, FlexAlign.Start, 0, first, second);
        panel.AlignContent = FlexAlignContent.Center;

        ShowAndLayout(panel, 200, 100);

        AssertBounds(first, x: 0, y: 40, width: 30, height: 20);
        AssertBounds(second, x: 30, y: 40, width: 40, height: 20);
    }

    [AvaloniaFact]
    public void Should_ignore_align_content_for_single_line_no_wrap_layout()
    {
        var first = Item(width: 30, height: 20);
        var second = Item(width: 40, height: 20);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second);
        panel.AlignContent = FlexAlignContent.Center;

        ShowAndLayout(panel, 200, 100);

        AssertBounds(first, x: 0, y: 0, width: 30, height: 20);
        AssertBounds(second, x: 30, y: 0, width: 40, height: 20);
    }

    [AvaloniaFact]
    public void Should_push_item_with_auto_main_axis_margin_to_the_end()
    {
        var first = Item(width: 40, height: 10);
        var second = Item(width: 30, height: 10);

        FlexPanel.SetMarginLeftAuto(second, true);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second);

        ShowAndLayout(panel, 200, 100);

        AssertBounds(first, x: 0, y: 0, width: 40, height: 10);
        AssertBounds(second, x: 170, y: 0, width: 30, height: 10);
    }

    [AvaloniaFact]
    public void Should_apply_cross_axis_auto_margins_before_align_items()
    {
        var item = Item(width: 40, height: 20);

        FlexPanel.SetMarginTopAuto(item, true);
        FlexPanel.SetMarginBottomAuto(item, true);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, item);

        ShowAndLayout(panel, 100, 100);

        AssertBounds(item, x: 0, y: 40, width: 40, height: 20);
    }

    [AvaloniaFact]
    public void Should_apply_single_cross_axis_auto_margin_to_push_item_to_the_end()
    {
        var item = Item(width: 40, height: 20);

        FlexPanel.SetMarginTopAuto(item, true);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Center, 0, item);

        ShowAndLayout(panel, 100, 100);

        AssertBounds(item, x: 0, y: 80, width: 40, height: 20);
    }

    [AvaloniaFact]
    public void Should_push_item_with_auto_margin_in_column_layout_like_a_sticky_footer()
    {
        var content = Item(width: 80, height: 40);
        var footer = Item(width: 80, height: 30);

        FlexPanel.SetMarginTopAuto(footer, true);

        var panel = Panel(FlexDirection.Column, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, content, footer);

        ShowAndLayout(panel, 100, 200);

        AssertBounds(content, x: 0, y: 0, width: 80, height: 40);
        AssertBounds(footer, x: 0, y: 170, width: 80, height: 30);
    }

    [AvaloniaFact]
    public void Should_support_common_flex_auto_shorthand()
    {
        var first = Item(width: 50, height: 10);
        var second = Item(width: 50, height: 10);

        FlexPanel.SetFlex(first, FlexValue.Auto);
        FlexPanel.SetFlex(second, FlexValue.Auto);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second);

        ShowAndLayout(panel, 200, 100);

        AssertBounds(first, x: 0, y: 0, width: 100, height: 10);
        AssertBounds(second, x: 100, y: 0, width: 100, height: 10);
    }

    [AvaloniaFact]
    public void Should_support_common_flex_none_shorthand()
    {
        var first = Item(width: 120, height: 10);
        var second = Item(width: 120, height: 10);

        FlexPanel.SetFlex(first, FlexValue.None);
        FlexPanel.SetFlex(second, FlexValue.None);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second);

        ShowAndLayout(panel, 200, 100);

        AssertBounds(first, x: 0, y: 0, width: 120, height: 10);
        AssertBounds(second, x: 120, y: 0, width: 120, height: 10);
    }

    [AvaloniaFact]
    public void Should_honor_row_gap_and_column_gap_over_generic_gap()
    {
        var first = Item(width: 40, height: 20);
        var second = Item(width: 40, height: 20);
        var third = Item(width: 40, height: 20);

        var panel = Panel(FlexDirection.Row, FlexWrap.Wrap, FlexJustify.Start, FlexAlign.Start, 10, first, second, third);
        panel.ColumnGap = 5;
        panel.RowGap = 20;
        panel.AlignContent = FlexAlignContent.Start;

        ShowAndLayout(panel, 100, 120);

        AssertBounds(first, x: 0, y: 0, width: 40, height: 20);
        AssertBounds(second, x: 45, y: 0, width: 40, height: 20);
        AssertBounds(third, x: 0, y: 40, width: 40, height: 20);
    }

    [AvaloniaFact]
    public void Should_reverse_cross_axis_stacking_when_wrap_reverse_is_enabled()
    {
        var first = Item(width: 40, height: 20);
        var second = Item(width: 40, height: 20);
        var third = Item(width: 40, height: 20);

        var panel = Panel(FlexDirection.Row, FlexWrap.WrapReverse, FlexJustify.Start, FlexAlign.Start, 10, first, second, third);
        panel.AlignContent = FlexAlignContent.Start;

        ShowAndLayout(panel, 100, 100);

        AssertBounds(first, x: 0, y: 80, width: 40, height: 20);
        AssertBounds(second, x: 50, y: 80, width: 40, height: 20);
        AssertBounds(third, x: 0, y: 50, width: 40, height: 20);
    }

    [AvaloniaFact]
    public void Should_align_shared_baselines_using_official_baseline_offset_data()
    {
        var first = Item(width: 40, height: 20);
        var second = Item(width: 40, height: 30);

        TextBlock.SetBaselineOffset(first, 15);
        TextBlock.SetBaselineOffset(second, 10);

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Baseline, 0, first, second);

        ShowAndLayout(panel, 100, 100);

        AssertBounds(first, x: 0, y: 0, width: 40, height: 20);
        AssertBounds(second, x: 40, y: 5, width: 40, height: 30);
        Assert.Equal(first.Bounds.Y + TextBlock.GetBaselineOffset(first), second.Bounds.Y + TextBlock.GetBaselineOffset(second), 5);
    }

    [AvaloniaFact]
    public void Should_expand_line_cross_size_when_baseline_alignment_requires_more_space()
    {
        var first = Item(width: 40, height: 20);
        var second = Item(width: 40, height: 30);

        TextBlock.SetBaselineOffset(first, 15);
        TextBlock.SetBaselineOffset(second, 5);

        var panel = Panel(FlexDirection.Row, FlexWrap.Wrap, FlexJustify.Start, FlexAlign.Baseline, 0, first, second);
        panel.AlignContent = FlexAlignContent.Start;

        ShowAndLayout(panel, 200, 100);

        Assert.Equal(40, panel.DesiredSize.Height, 5);
        AssertBounds(first, x: 0, y: 0, width: 40, height: 20);
        AssertBounds(second, x: 40, y: 10, width: 40, height: 30);
        Assert.Equal(first.Bounds.Y + TextBlock.GetBaselineOffset(first), second.Bounds.Y + TextBlock.GetBaselineOffset(second), 5);
    }

    [AvaloniaFact]
    public void Should_layout_common_vertical_stack_with_centered_justification()
    {
        var first = Item(width: 40, height: 20);
        var second = Item(width: 40, height: 30);

        var panel = Panel(FlexDirection.Column, FlexWrap.NoWrap, FlexJustify.Center, FlexAlign.Start, 10, first, second);

        ShowAndLayout(panel, 100, 100);

        AssertBounds(first, x: 0, y: 20, width: 40, height: 20);
        AssertBounds(second, x: 0, y: 50, width: 40, height: 30);
    }

    [AvaloniaFact]
    public void Should_keep_flex_box_alias_as_a_backward_compatible_entry_point()
    {
        var first = Item(width: 50, height: 10);
        var second = Item(width: 50, height: 10);

        FlexBox.SetGrow(first, 1);
        FlexBox.SetGrow(second, 1);

        var panel = new FlexBox
        {
            Direction = FlexDirection.Row,
            AlignItems = FlexAlign.Start
        };

        panel.Children.Add(first);
        panel.Children.Add(second);

        ShowAndLayout(panel, 200, 100);

        Assert.Same(FlexPanel.GrowProperty, FlexBox.GrowProperty);
        Assert.Equal(1, FlexPanel.GetGrow(first));
        AssertBounds(first, x: 0, y: 0, width: 100, height: 10);
        AssertBounds(second, x: 100, y: 0, width: 100, height: 10);
    }

    private static FlexPanel Panel(
        FlexDirection direction,
        FlexWrap wrap = FlexWrap.NoWrap,
        FlexJustify justifyContent = FlexJustify.Start,
        FlexAlign alignItems = FlexAlign.Start,
        double gap = 0,
        params Control[] children)
    {
        var panel = new FlexPanel
        {
            Direction = direction,
            Wrap = wrap,
            JustifyContent = justifyContent,
            AlignItems = alignItems,
            Gap = gap
        };

        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }

    private static Control Item(double width, double height) => new FixedSizeControl(width, height);

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

    private static void AssertBounds(Control control, double x, double y, double width, double height)
    {
        Assert.Equal(x, control.Bounds.X, 5);
        Assert.Equal(y, control.Bounds.Y, 5);
        Assert.Equal(width, control.Bounds.Width, 5);
        Assert.Equal(height, control.Bounds.Height, 5);
    }

    [AvaloniaFact]
    public void Should_remeasure_children_with_resolved_size_when_basis_is_zero()
    {
        // A control that adapts its height based on available width (like a wrapping TextBlock).
        // With 100+ width it's 20px tall; with less than 10px it wraps to 80px tall.
        var first = new WidthDependentControl(minMainForCompact: 100, compactCross: 20, expandedCross: 80);
        var second = new WidthDependentControl(minMainForCompact: 100, compactCross: 20, expandedCross: 80);

        FlexPanel.SetGrow(first, 1);
        FlexPanel.SetBasis(first, FlexBasis.Pixels(0));
        FlexPanel.SetGrow(second, 1);
        FlexPanel.SetBasis(second, FlexBasis.Pixels(0));

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Start, 0, first, second);

        ShowAndLayout(panel, 300, 200);

        // Each child gets 150px width (300 / 2), well above the 100px threshold.
        // After re-measure with resolved width, they should report 20px height (compact).
        AssertBounds(first, x: 0, y: 0, width: 150, height: 20);
        AssertBounds(second, x: 150, y: 0, width: 150, height: 20);
    }

    [AvaloniaFact]
    public void Should_remeasure_nested_stackpanel_children_with_resolved_size_when_basis_is_zero()
    {
        // Simulates the real-world scenario: StackPanel with children inside a Border,
        // using flex-basis: 0 with grow to get equal-width columns.
        var stackPanel1 = new StackPanel();
        stackPanel1.Children.Add(new FixedSizeControl(80, 30));
        stackPanel1.Children.Add(new FixedSizeControl(60, 20));
        var border1 = new Border { Child = stackPanel1 };

        var stackPanel2 = new StackPanel();
        stackPanel2.Children.Add(new FixedSizeControl(40, 25));
        stackPanel2.Children.Add(new FixedSizeControl(70, 15));
        var border2 = new Border { Child = stackPanel2 };

        FlexPanel.SetGrow(border1, 1);
        FlexPanel.SetBasis(border1, FlexBasis.Pixels(0));
        FlexPanel.SetGrow(border2, 1);
        FlexPanel.SetBasis(border2, FlexBasis.Pixels(0));

        var panel = Panel(FlexDirection.Row, FlexWrap.NoWrap, FlexJustify.Start, FlexAlign.Stretch, 0, border1, border2);

        ShowAndLayout(panel, 300, 200);

        // Both borders should get equal width (150 each) and the StackPanel content should be visible
        Assert.Equal(150, border1.Bounds.Width, 1);
        Assert.Equal(150, border2.Bounds.Width, 1);
        // The StackPanel should measure correctly with the resolved width:
        // StackPanel1 height = 30 + 20 = 50, StackPanel2 height = 25 + 15 = 40
        Assert.Equal(50, stackPanel1.DesiredSize.Height, 1);
        Assert.Equal(40, stackPanel2.DesiredSize.Height, 1);
    }

    private sealed class FixedSizeControl(double width, double height) : Control
    {
        protected override Size MeasureOverride(Size availableSize) => new(width, height);
    }

    /// <summary>
    /// A control whose cross-axis size depends on the available main-axis size,
    /// simulating wrapping behavior (like TextBlock with text wrapping).
    /// </summary>
    private sealed class WidthDependentControl(double minMainForCompact, double compactCross, double expandedCross) : Control
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            var availableMain = availableSize.Width;
            var crossSize = availableMain >= minMainForCompact ? compactCross : expandedCross;
            return new Size(Math.Min(minMainForCompact, availableMain), crossSize);
        }
    }
}
