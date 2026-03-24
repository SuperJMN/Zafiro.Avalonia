using CSharpFunctionalExtensions;

namespace Zafiro.Avalonia.Controls.Panels.Blueprint;

/// <summary>
/// Parses a text-based grid template DSL into a <see cref="GridTemplate"/>.
/// <para>
/// Syntax: space-separated tokens per row. Rows separated by newlines or '/'.
/// Token = non-negative integer (child index) or '.' (empty cell).
/// Each child index must form a complete rectangle in the grid.
/// </para>
/// </summary>
public static class BlueprintParser
{
    private const string EmptyCell = ".";

    public static Result<GridTemplate> Parse(string? blueprint)
    {
        if (string.IsNullOrWhiteSpace(blueprint))
        {
            return Result.Failure<GridTemplate>("Blueprint text is empty.");
        }

        return Tokenize(blueprint)
            .Bind(Validate)
            .Bind(ExtractPlacements);
    }

    private static Result<int?[,]> Tokenize(string blueprint)
    {
        var lines = blueprint
            .Replace("/", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => l.Length > 0)
            .ToList();

        if (lines.Count == 0)
        {
            return Result.Failure<int?[,]>("Blueprint contains no rows.");
        }

        var rows = new List<int?[]>();

        foreach (var line in lines)
        {
            var tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var row = new int?[tokens.Length];

            for (var col = 0; col < tokens.Length; col++)
            {
                var token = tokens[col];

                if (token == EmptyCell)
                {
                    row[col] = null;
                    continue;
                }

                if (!int.TryParse(token, out var index) || index < 0)
                {
                    return Result.Failure<int?[,]>(
                        $"Invalid token '{token}' at row {rows.Count}, column {col}. Expected a non-negative integer or '.'.");
                }

                row[col] = index;
            }

            rows.Add(row);
        }

        var columnCount = rows[0].Length;

        for (var r = 1; r < rows.Count; r++)
        {
            if (rows[r].Length != columnCount)
            {
                return Result.Failure<int?[,]>(
                    $"Row {r} has {rows[r].Length} columns but row 0 has {columnCount}. All rows must have the same number of columns.");
            }
        }

        var grid = new int?[rows.Count, columnCount];

        for (var r = 0; r < rows.Count; r++)
        {
            for (var c = 0; c < columnCount; c++)
            {
                grid[r, c] = rows[r][c];
            }
        }

        return Result.Success(grid);
    }

    private static Result<int?[,]> Validate(int?[,] grid)
    {
        var rowCount = grid.GetLength(0);
        var colCount = grid.GetLength(1);

        // Group cells by child index
        var cellsByIndex = new Dictionary<int, List<(int Row, int Col)>>();

        for (var r = 0; r < rowCount; r++)
        {
            for (var c = 0; c < colCount; c++)
            {
                var value = grid[r, c];
                if (value is null)
                {
                    continue;
                }

                var index = value.Value;

                if (!cellsByIndex.TryGetValue(index, out var cells))
                {
                    cells = new List<(int, int)>();
                    cellsByIndex[index] = cells;
                }

                cells.Add((r, c));
            }
        }

        // Validate each index forms a complete rectangle
        foreach (var (index, cells) in cellsByIndex)
        {
            var minRow = cells.Min(p => p.Row);
            var maxRow = cells.Max(p => p.Row);
            var minCol = cells.Min(p => p.Col);
            var maxCol = cells.Max(p => p.Col);

            var expectedCount = (maxRow - minRow + 1) * (maxCol - minCol + 1);

            if (cells.Count != expectedCount)
            {
                return Result.Failure<int?[,]>(
                    $"Child {index} does not form a complete rectangle. Found {cells.Count} cells but expected {expectedCount} for the bounding box [{minRow},{minCol}]→[{maxRow},{maxCol}].");
            }
        }

        return Result.Success(grid);
    }

    private static Result<GridTemplate> ExtractPlacements(int?[,] grid)
    {
        var rowCount = grid.GetLength(0);
        var colCount = grid.GetLength(1);

        var cellsByIndex = new Dictionary<int, (int MinRow, int MinCol, int MaxRow, int MaxCol)>();

        for (var r = 0; r < rowCount; r++)
        {
            for (var c = 0; c < colCount; c++)
            {
                var value = grid[r, c];
                if (value is null)
                {
                    continue;
                }

                var index = value.Value;

                if (cellsByIndex.TryGetValue(index, out var bounds))
                {
                    cellsByIndex[index] = (
                        Math.Min(bounds.MinRow, r),
                        Math.Min(bounds.MinCol, c),
                        Math.Max(bounds.MaxRow, r),
                        Math.Max(bounds.MaxCol, c));
                }
                else
                {
                    cellsByIndex[index] = (r, c, r, c);
                }
            }
        }

        var placements = cellsByIndex
            .Select(kvp => new ChildPlacement(
                kvp.Key,
                kvp.Value.MinRow,
                kvp.Value.MinCol,
                kvp.Value.MaxRow - kvp.Value.MinRow + 1,
                kvp.Value.MaxCol - kvp.Value.MinCol + 1))
            .OrderBy(p => p.ChildIndex)
            .ToList();

        return Result.Success(new GridTemplate(rowCount, colCount, placements));
    }
}