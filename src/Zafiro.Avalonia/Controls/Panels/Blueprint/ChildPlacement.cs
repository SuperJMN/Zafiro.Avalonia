namespace Zafiro.Avalonia.Controls.Panels.Blueprint;

/// <summary>
/// Describes where a single child should be placed within the grid.
/// </summary>
public sealed record ChildPlacement(int ChildIndex, int Row, int Column, int RowSpan, int ColumnSpan);