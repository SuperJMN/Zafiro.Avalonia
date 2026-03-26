namespace Zafiro.Avalonia.Controls.Panels;

/// <summary>
/// UniformStripPanel lays out its visible children in a single horizontal row where
/// all items have the same width. The panel "snaps" the item width so that an integer
/// number of items fits in the available width without showing partial items.
///
/// If <see cref="ItemMaxWidth"/> is set and the available width would produce items wider
/// than this value, the panel increases the number of items per view (when possible) to keep
/// each item at or below <see cref="ItemMaxWidth"/>. If there are not enough children to
/// satisfy that constraint, items are arranged with <see cref="ItemMaxWidth"/> and the strip
/// may not fill the entire width (leaving empty space at the end), but it will never show
/// partial items in the visible region.
///
/// If <see cref="ItemMinWidth"/> is set and the computed item width would fall below this value,
/// the panel reduces the number of items per view (down to one) to respect the minimum when possible.
/// If the available width is itself less than <see cref="ItemMinWidth"/>, the panel prefers
/// avoiding partial items and will use the available width for the single visible item.
///
/// Spacing is applied only between items, not at the edges.
/// </summary>
public class UniformStripPanel : Panel
{
    public static readonly StyledProperty<double> ItemMinWidthProperty =
        AvaloniaProperty.Register<UniformStripPanel, double>(nameof(ItemMinWidth), 0d);

    public static readonly StyledProperty<double> ItemMaxWidthProperty =
        AvaloniaProperty.Register<UniformStripPanel, double>(nameof(ItemMaxWidth), double.PositiveInfinity);

    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<UniformStripPanel, double>(nameof(ItemSpacing), 0d);

    static UniformStripPanel()
    {
        AffectsMeasure<UniformStripPanel>(ItemMinWidthProperty, ItemMaxWidthProperty, ItemSpacingProperty);
    }

    public double ItemMinWidth
    {
        get => GetValue(ItemMinWidthProperty);
        set => SetValue(ItemMinWidthProperty, value);
    }

    public double ItemMaxWidth
    {
        get => GetValue(ItemMaxWidthProperty);
        set => SetValue(ItemMaxWidthProperty, value);
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var children = Children.Where(c => c.IsVisible).ToList();
        if (children.Count == 0)
        {
            return new Size(0, 0);
        }

        double spacing = Math.Max(0, ItemSpacing);
        double min = Math.Max(0, ItemMinWidth);
        double max = ItemMaxWidth <= 0 ? double.PositiveInfinity : ItemMaxWidth;
        if (max < min) max = min;

        double heightConstraint = availableSize.Height;

        if (double.IsInfinity(availableSize.Width))
        {
            // Unconstrained width: choose a uniform width derived from children's natural sizes
            foreach (var child in children)
            {
                child.Measure(new Size(double.PositiveInfinity, heightConstraint));
            }

            double naturalMax = children.Max(c => c.DesiredSize.Width);
            double uniformWidth = ClampUpper(naturalMax, max);
            uniformWidth = Math.Max(uniformWidth, min);

            double maxHeight = 0;
            foreach (var child in children)
            {
                child.Measure(new Size(uniformWidth, heightConstraint));
                maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
            }

            double totalWidth = uniformWidth * children.Count + spacing * Math.Max(0, children.Count - 1);
            return new Size(totalWidth, maxHeight);
        }
        else
        {
            double containerWidth = availableSize.Width;
            ComputeLayout(containerWidth, children.Count, min, max, spacing,
                out var columns, out var itemWidth, out _);

            // Measure all children to respect the layout contract,
            // but compute the height using only the items visible in the first page (the first 'columns').
            int visibleInView = Math.Max(0, Math.Min(columns, children.Count));

            double maxHeight = 0;
            int index = 0;
            foreach (var child in children)
            {
                child.Measure(new Size(itemWidth, heightConstraint));
                if (index < visibleInView)
                {
                    maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
                }

                index++;
            }

            // Snap to available width so that Arrange won't show partial items.
            return new Size(containerWidth, maxHeight);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children.Where(c => c.IsVisible).ToList();
        if (children.Count == 0)
        {
            return finalSize;
        }

        double spacing = Math.Max(0, ItemSpacing);
        double min = Math.Max(0, ItemMinWidth);
        double max = ItemMaxWidth <= 0 ? double.PositiveInfinity : ItemMaxWidth;
        if (max < min) max = min;

        double containerWidth = finalSize.Width;
        ComputeLayout(containerWidth, children.Count, min, max, spacing,
            out var columns, out var itemWidth, out _);

        double x = 0;
        foreach (var child in children)
        {
            child.Arrange(new Rect(x, 0, itemWidth, finalSize.Height));
            x += itemWidth + spacing;
        }

        return finalSize;
    }

    private static void ComputeLayout(
        double containerWidth,
        int visibleCount,
        double min,
        double max,
        double spacing,
        out int columns,
        out double itemWidth,
        out double usedWidth)
    {
        if (visibleCount <= 0)
        {
            columns = 0;
            itemWidth = 0;
            usedWidth = 0;
            return;
        }

        // Start with the number of columns required to keep item width <= max.
        // When max is unconstrained, try to fit all items.
        int cByMax = double.IsInfinity(max)
            ? visibleCount
            : Math.Max(1, (int)Math.Ceiling((containerWidth + spacing) / (max + spacing)));

        int c = Math.Max(1, cByMax);
        c = Math.Min(c, visibleCount); // can't have more columns than visible items

        double iw = ComputeItemWidth(containerWidth, c, spacing);

        // If width per item is below the minimum and we can reduce columns, do so until we reach min or a single column
        while (iw < min && c > 1)
        {
            c--;
            iw = ComputeItemWidth(containerWidth, c, spacing);
        }

        // If width per item is above max and we have more items to bring into view, increase columns
        while (!double.IsInfinity(max) && iw > max && c < visibleCount)
        {
            c++;
            iw = ComputeItemWidth(containerWidth, c, spacing);
        }

        // Do NOT force iw up to min if container is narrower than min; prefer avoiding partial items.
        // If we still exceed max but can't increase columns (too few items), clamp down to max and allow leftover space.
        if (!double.IsInfinity(max) && iw > max)
        {
            iw = max;
        }

        columns = Math.Max(1, c);
        itemWidth = Math.Max(0, iw);
        usedWidth = columns * itemWidth + spacing * Math.Max(0, columns - 1);
    }

    private static double ComputeItemWidth(double containerWidth, int columns, double spacing)
    {
        if (columns <= 0)
        {
            return 0;
        }

        var gaps = Math.Max(0, columns - 1);
        return Math.Max(0, (containerWidth - gaps * spacing) / columns);
    }

    private static double ClampUpper(double value, double max)
    {
        if (double.IsInfinity(max)) return value;
        return Math.Min(value, max);
    }
}