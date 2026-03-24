namespace Zafiro.Avalonia.Controls.Panels;

/// <summary>
/// Size classification resolved by <see cref="SemanticPanel"/> from the available width.
/// </summary>
public enum SizeClass
{
    /// <summary>Narrow layout (&lt; CompactBreakpoint). Everything stacks vertically.</summary>
    Compact,

    /// <summary>Medium layout (between CompactBreakpoint and ExpandedBreakpoint). Side-by-side without sidebar column.</summary>
    Medium,

    /// <summary>Wide layout (≥ ExpandedBreakpoint). Full desktop layout with sidebar column.</summary>
    Expanded,
}