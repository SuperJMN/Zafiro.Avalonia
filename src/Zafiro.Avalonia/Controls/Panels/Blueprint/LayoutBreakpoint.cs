namespace Zafiro.Avalonia.Controls.Panels.Blueprint;

/// <summary>
/// Defines a grid layout template that activates when the container meets size constraints.
/// </summary>
public class LayoutBreakpoint : AvaloniaObject
{
    public static readonly StyledProperty<double> MinWidthProperty =
        AvaloniaProperty.Register<LayoutBreakpoint, double>(nameof(MinWidth));

    public static readonly StyledProperty<double> MinHeightProperty =
        AvaloniaProperty.Register<LayoutBreakpoint, double>(nameof(MinHeight));

    public static readonly StyledProperty<string?> BlueprintProperty =
        AvaloniaProperty.Register<LayoutBreakpoint, string?>(nameof(Blueprint));

    /// <summary>
    /// Minimum container width for this breakpoint to activate. Default 0 (always matches width).
    /// </summary>
    public double MinWidth
    {
        get => GetValue(MinWidthProperty);
        set => SetValue(MinWidthProperty, value);
    }

    /// <summary>
    /// Minimum container height for this breakpoint to activate. Default 0 (always matches height).
    /// </summary>
    public double MinHeight
    {
        get => GetValue(MinHeightProperty);
        set => SetValue(MinHeightProperty, value);
    }

    /// <summary>
    /// The grid template text. Space-separated tokens per row, rows separated by newlines or '/'.
    /// </summary>
    public string? Blueprint
    {
        get => GetValue(BlueprintProperty);
        set => SetValue(BlueprintProperty, value);
    }

    public bool Matches(double availableWidth, double availableHeight)
    {
        return availableWidth >= MinWidth && availableHeight >= MinHeight;
    }
}