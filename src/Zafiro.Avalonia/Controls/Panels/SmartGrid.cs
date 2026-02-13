namespace Zafiro.Avalonia.Controls.Panels;

public class SmartGrid : Panel
{
    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<SmartGrid, double>(nameof(RowSpacing));

    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<SmartGrid, double>(nameof(ColumnSpacing));

    public static readonly StyledProperty<RowDefinitions> RowDefinitionsProperty =
        AvaloniaProperty.Register<SmartGrid, RowDefinitions>(nameof(RowDefinitions));

    public static readonly StyledProperty<ColumnDefinitions> ColumnDefinitionsProperty =
        AvaloniaProperty.Register<SmartGrid, ColumnDefinitions>(nameof(ColumnDefinitions));

    /// <summary>
    /// Defines the Row attached property.
    /// </summary>
    public static readonly AttachedProperty<int> RowProperty =
        AvaloniaProperty.RegisterAttached<SmartGrid, Control, int>("Row");

    /// <summary>
    /// Defines the Column attached property.
    /// </summary>
    public static readonly AttachedProperty<int> ColumnProperty =
        AvaloniaProperty.RegisterAttached<SmartGrid, Control, int>("Column");

    /// <summary>
    /// Defines the RowSpan attached property.
    /// </summary>
    public static readonly AttachedProperty<int> RowSpanProperty =
        AvaloniaProperty.RegisterAttached<SmartGrid, Control, int>("RowSpan", 1);

    /// <summary>
    /// Defines the ColumnSpan attached property.
    /// </summary>
    public static readonly AttachedProperty<int> ColumnSpanProperty =
        AvaloniaProperty.RegisterAttached<SmartGrid, Control, int>("ColumnSpan", 1);

    public SmartGrid()
    {
        RowDefinitions = new RowDefinitions();
        ColumnDefinitions = new ColumnDefinitions();
    }

    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    public double ColumnSpacing
    {
        get => GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    public RowDefinitions RowDefinitions
    {
        get => GetValue(RowDefinitionsProperty);
        set => SetValue(RowDefinitionsProperty, value);
    }

    public ColumnDefinitions ColumnDefinitions
    {
        get => GetValue(ColumnDefinitionsProperty);
        set => SetValue(ColumnDefinitionsProperty, value);
    }

    public static int GetRow(Control element) => element.GetValue(RowProperty);
    public static void SetRow(Control element, int value) => element.SetValue(RowProperty, value);

    public static int GetColumn(Control element) => element.GetValue(ColumnProperty);
    public static void SetColumn(Control element, int value) => element.SetValue(ColumnProperty, value);

    public static int GetRowSpan(Control element) => element.GetValue(RowSpanProperty);
    public static void SetRowSpan(Control element, int value) => element.SetValue(RowSpanProperty, value);

    public static int GetColumnSpan(Control element) => element.GetValue(ColumnSpanProperty);
    public static void SetColumnSpan(Control element, int value) => element.SetValue(ColumnSpanProperty, value);

    private static int ClampTrackIndex(int index, int trackCount) => Math.Clamp(index, 0, trackCount - 1);

    private LayoutInfo CalculateLayout(Size availableSize)
    {
        var rowDefs = RowDefinitions;
        var colDefs = ColumnDefinitions;
        var rowCount = Math.Max(1, rowDefs.Count);
        var colCount = Math.Max(1, colDefs.Count);

        // 1. Determine visibility of tracks
        var visibleRows = new bool[rowCount];
        var visibleCols = new bool[colCount];

        if (rowDefs.Count == 0) visibleRows[0] = true;
        if (colDefs.Count == 0) visibleCols[0] = true;

        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;

            var r = ClampTrackIndex(GetRow(child), rowCount);
            var c = ClampTrackIndex(GetColumn(child), colCount);
            var rs = Math.Max(1, GetRowSpan(child));
            var cs = Math.Max(1, GetColumnSpan(child));

            for (int i = r; i < r + rs && i < visibleRows.Length; i++) visibleRows[i] = true;
            for (int i = c; i < c + cs && i < visibleCols.Length; i++) visibleCols[i] = true;
        }

        // 2. Calculate required spacing
        double totalRowSpacing = 0;
        int activeRowCount = 0;
        for (int i = 0; i < visibleRows.Length; i++)
        {
            if (visibleRows[i])
            {
                if (activeRowCount > 0) totalRowSpacing += RowSpacing;
                activeRowCount++;
            }
        }

        double totalColSpacing = 0;
        int activeColCount = 0;
        for (int i = 0; i < visibleCols.Length; i++)
        {
            if (visibleCols[i])
            {
                if (activeColCount > 0) totalColSpacing += ColumnSpacing;
                activeColCount++;
            }
        }

        // 3. Measure children and calculate track sizes (Auto/Pixel)
        var rowSizes = new double[visibleRows.Length];
        var colSizes = new double[visibleCols.Length];

        // Initialize Pixel sizes
        for (int i = 0; i < rowCount; i++)
        {
            if (visibleRows[i] && rowDefs.Count > 0 && rowDefs[i].Height.IsAbsolute)
            {
                rowSizes[i] = rowDefs[i].Height.Value;
            }
        }

        for (int i = 0; i < colCount; i++)
        {
            if (visibleCols[i] && colDefs.Count > 0 && colDefs[i].Width.IsAbsolute)
            {
                colSizes[i] = colDefs[i].Width.Value;
            }
        }

        // Measure children to expand Auto tracks
        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;

            var r = ClampTrackIndex(GetRow(child), rowCount);
            var c = ClampTrackIndex(GetColumn(child), colCount);

            var isRowAuto = rowDefs.Count == 0 || rowDefs[r].Height.IsAuto;
            var isColAuto = colDefs.Count == 0 || colDefs[c].Width.IsAuto;

            if (isRowAuto || isColAuto)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                if (isRowAuto) rowSizes[r] = Math.Max(rowSizes[r], child.DesiredSize.Height);
                if (isColAuto) colSizes[c] = Math.Max(colSizes[c], child.DesiredSize.Width);
            }
        }

        // 4. Resolve Star tracks
        double availableHeight = availableSize.Height - totalRowSpacing;
        double availableWidth = availableSize.Width - totalColSpacing;

        for (int i = 0; i < rowSizes.Length; i++)
            if (visibleRows[i])
                availableHeight -= rowSizes[i];
        for (int i = 0; i < colSizes.Length; i++)
            if (visibleCols[i])
                availableWidth -= colSizes[i];

        availableHeight = Math.Max(0, availableHeight);
        availableWidth = Math.Max(0, availableWidth);

        double totalStarsH = 0;
        double totalStarsW = 0;

        for (int i = 0; i < rowCount; i++)
            if (visibleRows[i] && rowDefs.Count > 0 && rowDefs[i].Height.IsStar)
                totalStarsH += rowDefs[i].Height.Value;

        for (int i = 0; i < colCount; i++)
            if (visibleCols[i] && colDefs.Count > 0 && colDefs[i].Width.IsStar)
                totalStarsW += colDefs[i].Width.Value;

        if (totalStarsH > 0)
        {
            for (int i = 0; i < rowCount; i++)
                if (visibleRows[i] && rowDefs.Count > 0 && rowDefs[i].Height.IsStar)
                    rowSizes[i] = (rowDefs[i].Height.Value / totalStarsH) * availableHeight;
        }

        if (totalStarsW > 0)
        {
            for (int i = 0; i < colCount; i++)
                if (visibleCols[i] && colDefs.Count > 0 && colDefs[i].Width.IsStar)
                    colSizes[i] = (colDefs[i].Width.Value / totalStarsW) * availableWidth;
        }

        return new LayoutInfo
        {
            VisibleRows = visibleRows,
            VisibleCols = visibleCols,
            RowSizes = rowSizes,
            ColSizes = colSizes,
            TotalRowSpacing = totalRowSpacing,
            TotalColSpacing = totalColSpacing
        };
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var layout = CalculateLayout(availableSize);
        var rowDefs = RowDefinitions;
        var colDefs = ColumnDefinitions;
        var rowCount = layout.RowSizes.Length;
        var colCount = layout.ColSizes.Length;

        // 5. Final Measure for Star children
        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;

            var r = ClampTrackIndex(GetRow(child), rowCount);
            var c = ClampTrackIndex(GetColumn(child), colCount);

            var isRowStar = rowDefs.Count > 0 && rowDefs[r].Height.IsStar;
            var isColStar = colDefs.Count > 0 && colDefs[c].Width.IsStar;

            if (isRowStar || isColStar)
            {
                var h = layout.RowSizes[r];
                var w = layout.ColSizes[c];
                child.Measure(new Size(w, h));
            }
        }

        var totalH = layout.TotalRowSpacing;
        for (int i = 0; i < layout.RowSizes.Length; i++)
        {
            if (layout.VisibleRows[i]) totalH += layout.RowSizes[i];
        }

        var totalW = layout.TotalColSpacing;
        for (int i = 0; i < layout.ColSizes.Length; i++)
        {
            if (layout.VisibleCols[i]) totalW += layout.ColSizes[i];
        }

        return new Size(totalW, totalH);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var layout = CalculateLayout(finalSize);

        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;

            var r = ClampTrackIndex(GetRow(child), layout.VisibleRows.Length);
            var c = ClampTrackIndex(GetColumn(child), layout.VisibleCols.Length);
            var rs = Math.Max(1, GetRowSpan(child));
            var cs = Math.Max(1, GetColumnSpan(child));

            // Calculate position
            double x = 0;
            double y = 0;

            // X position
            int activeColCount = 0;
            for (int i = 0; i < c; i++)
            {
                x += layout.ColSizes[i];
                if (layout.VisibleCols[i]) activeColCount++;
            }

            // Add spacing based on active columns before this one
            if (activeColCount > 0) x += activeColCount * ColumnSpacing;


            // Y position
            int activeRowCount = 0;
            for (int i = 0; i < r; i++)
            {
                y += layout.RowSizes[i];
                if (layout.VisibleRows[i]) activeRowCount++;
            }

            if (activeRowCount > 0) y += activeRowCount * RowSpacing;

            // Size
            double w = 0;
            for (int i = c; i < c + cs && i < layout.ColSizes.Length; i++)
            {
                w += layout.ColSizes[i];
            }

            // Add spacing inside span
            int visibleInSpanW = 0;
            for (int i = c; i < c + cs && i < layout.VisibleCols.Length; i++)
                if (layout.VisibleCols[i])
                    visibleInSpanW++;
            if (visibleInSpanW > 1) w += (visibleInSpanW - 1) * ColumnSpacing;

            double h = 0;
            for (int i = r; i < r + rs && i < layout.RowSizes.Length; i++)
            {
                h += layout.RowSizes[i];
            }

            int visibleInSpanH = 0;
            for (int i = r; i < r + rs && i < layout.VisibleRows.Length; i++)
                if (layout.VisibleRows[i])
                    visibleInSpanH++;
            if (visibleInSpanH > 1) h += (visibleInSpanH - 1) * RowSpacing;

            child.Arrange(new Rect(x, y, w, h));
        }

        return finalSize;
    }

    private class LayoutInfo
    {
        public bool[] VisibleRows { get; set; }
        public bool[] VisibleCols { get; set; }
        public double[] RowSizes { get; set; }
        public double[] ColSizes { get; set; }
        public double TotalRowSpacing { get; set; }
        public double TotalColSpacing { get; set; }
    }
}