using System.Collections.Specialized;
using Avalonia.Collections;

namespace Zafiro.Avalonia.Controls.Panels.Blueprint;

/// <summary>
/// A Panel that arranges children according to a visual text-based grid template,
/// with optional responsive breakpoints that swap layouts based on container size.
/// </summary>
public class BlueprintPanel : Panel
{
    public static readonly StyledProperty<string?> LayoutProperty =
        AvaloniaProperty.Register<BlueprintPanel, string?>(nameof(Layout));

    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<BlueprintPanel, double>(nameof(RowSpacing));

    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<BlueprintPanel, double>(nameof(ColumnSpacing));

    public static readonly DirectProperty<BlueprintPanel, AvaloniaList<LayoutBreakpoint>> LayoutsProperty =
        AvaloniaProperty.RegisterDirect<BlueprintPanel, AvaloniaList<LayoutBreakpoint>>(
            nameof(Layouts),
            o => o.Layouts);

    private string? cachedBlueprint;
    private GridTemplate? cachedTemplate;

    private AvaloniaList<LayoutBreakpoint> layouts = new();

    static BlueprintPanel()
    {
        AffectsMeasure<BlueprintPanel>(LayoutProperty, RowSpacingProperty, ColumnSpacingProperty);
        AffectsArrange<BlueprintPanel>(RowSpacingProperty, ColumnSpacingProperty);
    }

    public BlueprintPanel()
    {
        layouts.CollectionChanged += OnLayoutsChanged;
    }

    /// <summary>
    /// Single layout definition. Rows separated by '/' or newlines.
    /// Ignored when <see cref="Layouts"/> is non-empty.
    /// </summary>
    public string? Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
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

    /// <summary>
    /// Collection of responsive layout breakpoints. Evaluated in order; first match wins.
    /// </summary>
    public AvaloniaList<LayoutBreakpoint> Layouts
    {
        get => layouts;
        private set => SetAndRaise(LayoutsProperty, ref layouts, value);
    }

    private void OnLayoutsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateMeasure();
    }

    private string? ResolveBlueprint(double availableWidth, double availableHeight)
    {
        if (layouts.Count > 0)
        {
            var width = double.IsInfinity(availableWidth) ? double.MaxValue : availableWidth;
            var height = double.IsInfinity(availableHeight) ? double.MaxValue : availableHeight;

            foreach (var bp in layouts)
            {
                if (bp.Matches(width, height))
                {
                    return bp.Blueprint;
                }
            }

            return null;
        }

        return Layout;
    }

    private GridTemplate? GetTemplate(string? blueprint)
    {
        if (string.IsNullOrWhiteSpace(blueprint))
        {
            cachedBlueprint = null;
            cachedTemplate = null;
            return null;
        }

        if (blueprint == cachedBlueprint && cachedTemplate != null)
        {
            return cachedTemplate;
        }

        var result = BlueprintParser.Parse(blueprint);

        if (result.IsFailure)
        {
            cachedBlueprint = null;
            cachedTemplate = null;
            return null;
        }

        cachedBlueprint = blueprint;
        cachedTemplate = result.Value;
        return cachedTemplate;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var blueprint = ResolveBlueprint(availableSize.Width, availableSize.Height);
        var template = GetTemplate(blueprint);

        if (template == null)
        {
            return new Size();
        }

        var cellSize = CalculateCellSize(availableSize, template);

        foreach (var child in Children)
        {
            var placement = FindPlacement(template, child);

            if (placement == null)
            {
                child.Measure(new Size(0, 0));
                continue;
            }

            var childWidth = placement.ColumnSpan * cellSize.Width
                             + (placement.ColumnSpan - 1) * ColumnSpacing;
            var childHeight = placement.RowSpan * cellSize.Height
                              + (placement.RowSpan - 1) * RowSpacing;

            child.Measure(new Size(Math.Max(0, childWidth), Math.Max(0, childHeight)));
        }

        var totalWidth = template.Columns * cellSize.Width + (template.Columns - 1) * ColumnSpacing;
        var totalHeight = template.Rows * cellSize.Height + (template.Rows - 1) * RowSpacing;

        return new Size(Math.Max(0, totalWidth), Math.Max(0, totalHeight));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var blueprint = ResolveBlueprint(finalSize.Width, finalSize.Height);
        var template = GetTemplate(blueprint);

        if (template == null)
        {
            return finalSize;
        }

        var cellSize = CalculateCellSize(finalSize, template);

        foreach (var child in Children)
        {
            var placement = FindPlacement(template, child);

            if (placement == null)
            {
                child.Arrange(new Rect(0, 0, 0, 0));
                continue;
            }

            var x = placement.Column * (cellSize.Width + ColumnSpacing);
            var y = placement.Row * (cellSize.Height + RowSpacing);
            var width = placement.ColumnSpan * cellSize.Width
                        + (placement.ColumnSpan - 1) * ColumnSpacing;
            var height = placement.RowSpan * cellSize.Height
                         + (placement.RowSpan - 1) * RowSpacing;

            child.Arrange(new Rect(x, y, Math.Max(0, width), Math.Max(0, height)));
        }

        return finalSize;
    }

    private Size CalculateCellSize(Size availableSize, GridTemplate template)
    {
        var totalColSpacing = (template.Columns - 1) * ColumnSpacing;
        var totalRowSpacing = (template.Rows - 1) * RowSpacing;

        var cellWidth = double.IsInfinity(availableSize.Width)
            ? 0
            : (availableSize.Width - totalColSpacing) / template.Columns;

        var cellHeight = double.IsInfinity(availableSize.Height)
            ? 0
            : (availableSize.Height - totalRowSpacing) / template.Rows;

        return new Size(Math.Max(0, cellWidth), Math.Max(0, cellHeight));
    }

    private ChildPlacement? FindPlacement(GridTemplate template, Control child)
    {
        var index = Children.IndexOf(child);
        return template.Placements.FirstOrDefault(p => p.ChildIndex == index);
    }
}