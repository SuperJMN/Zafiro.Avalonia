namespace Zafiro.Avalonia.Controls.Panels;

/// <summary>
/// Wrapping panel that uses natural (auto) sizing when all items fit in a single row,
/// and switches to uniform item widths when wrapping occurs — preventing jagged rows.
/// </summary>
public class AutoWrapGrid : Panel
{
    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<AutoWrapGrid, double>(nameof(ColumnSpacing));

    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<AutoWrapGrid, double>(nameof(RowSpacing));

    private double[] naturalWidths = [];
    private double naturalMaxHeight;
    private bool isSingleRow;
    private int columns;
    private double uniformWidth;
    private double measuredAvailableWidth;

    public double ColumnSpacing
    {
        get => GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var visible = CollectVisible();
        if (visible.Length == 0) return default;

        double spacing = ColumnSpacing;
        double available = availableSize.Width;
        measuredAvailableWidth = available;

        MeasureNatural(visible, availableSize.Height);
        ComputeLayout(visible.Length, available, spacing);

        if (isSingleRow)
        {
            double width = Sum(naturalWidths, visible.Length) + Math.Max(0, visible.Length - 1) * spacing;
            return new Size(double.IsInfinity(available) ? width : Math.Min(width, available), naturalMaxHeight);
        }

        double rowHeight = RemeasureConstrained(visible, uniformWidth, availableSize.Height);
        int rows = (int)Math.Ceiling((double)visible.Length / columns);
        double totalHeight = rows * rowHeight + Math.Max(0, rows - 1) * RowSpacing;
        return new Size(available, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var visible = CollectVisible();
        if (visible.Length == 0) return finalSize;

        double colSpacing = ColumnSpacing;
        double rowSpacing = RowSpacing;

        if (Math.Abs(finalSize.Width - measuredAvailableWidth) > 0.5)
        {
            MeasureNatural(visible, finalSize.Height);
            ComputeLayout(visible.Length, finalSize.Width, colSpacing);
            if (!isSingleRow)
                RemeasureConstrained(visible, uniformWidth, finalSize.Height);
        }

        if (isSingleRow)
        {
            double rowHeight = MaxDesiredHeight(visible);
            double x = 0;
            for (int i = 0; i < visible.Length; i++)
            {
                if (i > 0) x += colSpacing;
                visible[i].Arrange(new Rect(x, 0, naturalWidths[i], rowHeight));
                x += naturalWidths[i];
            }

            return finalSize;
        }

        double maxRowHeight = MaxDesiredHeight(visible);
        double y = 0;
        for (int i = 0; i < visible.Length; i++)
        {
            int col = i % columns;
            if (col == 0 && i > 0) y += maxRowHeight + rowSpacing;
            double x = col * (uniformWidth + colSpacing);
            visible[i].Arrange(new Rect(x, y, uniformWidth, maxRowHeight));
        }

        return finalSize;
    }

    private void MeasureNatural(Control[] visible, double availableHeight)
    {
        if (naturalWidths.Length < visible.Length)
            naturalWidths = new double[visible.Length];

        naturalMaxHeight = 0;
        for (int i = 0; i < visible.Length; i++)
        {
            visible[i].Measure(new Size(double.PositiveInfinity, availableHeight));
            naturalWidths[i] = visible[i].DesiredSize.Width;
            naturalMaxHeight = Math.Max(naturalMaxHeight, visible[i].DesiredSize.Height);
        }
    }

    private void ComputeLayout(int count, double available, double spacing)
    {
        double totalNatural = Sum(naturalWidths, count) + Math.Max(0, count - 1) * spacing;
        isSingleRow = double.IsInfinity(available) || totalNatural <= available;
        if (isSingleRow)
        {
            columns = count;
            uniformWidth = 0;
            return;
        }

        double maxNatural = Max(naturalWidths, count);
        columns = Math.Max(1, (int)Math.Floor((available + spacing) / (maxNatural + spacing)));
        uniformWidth = (available - Math.Max(0, columns - 1) * spacing) / columns;
    }

    private static double RemeasureConstrained(Control[] visible, double width, double availableHeight)
    {
        double maxHeight = 0;
        var constraint = new Size(width, availableHeight);
        foreach (var child in visible)
        {
            child.Measure(constraint);
            maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
        }

        return maxHeight;
    }

    private static double MaxDesiredHeight(Control[] visible)
    {
        double max = 0;
        foreach (var child in visible)
            max = Math.Max(max, child.DesiredSize.Height);
        return max;
    }

    private Control[] CollectVisible()
    {
        int count = 0;
        foreach (var child in Children)
            if (child is Control { IsVisible: true })
                count++;

        var result = new Control[count];
        int idx = 0;
        foreach (var child in Children)
            if (child is Control c && c.IsVisible)
                result[idx++] = c;
        return result;
    }

    private static double Sum(double[] values, int count)
    {
        double total = 0;
        for (int i = 0; i < count; i++) total += values[i];
        return total;
    }

    private static double Max(double[] values, int count)
    {
        double max = 0;
        for (int i = 0; i < count; i++)
            if (values[i] > max) max = values[i];
        return max;
    }
}
