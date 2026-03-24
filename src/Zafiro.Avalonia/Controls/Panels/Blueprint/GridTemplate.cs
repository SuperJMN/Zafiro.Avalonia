namespace Zafiro.Avalonia.Controls.Panels.Blueprint;

/// <summary>
/// Immutable representation of a parsed grid layout template.
/// </summary>
public sealed class GridTemplate
{
    public GridTemplate(int rows, int columns, IReadOnlyList<ChildPlacement> placements)
    {
        Rows = rows;
        Columns = columns;
        Placements = placements;
    }

    public int Rows { get; }
    public int Columns { get; }
    public IReadOnlyList<ChildPlacement> Placements { get; }
}