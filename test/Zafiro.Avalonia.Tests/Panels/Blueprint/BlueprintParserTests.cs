using Zafiro.Avalonia.Controls.Panels.Blueprint;

namespace Zafiro.Avalonia.Tests.Panels.Blueprint;

public class BlueprintParserTests
{
    [Fact]
    public void Simple_2x2_grid()
    {
        var result = BlueprintParser.Parse("0 1\n2 3");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Equal(2, t.Rows);
        Assert.Equal(2, t.Columns);
        Assert.Equal(4, t.Placements.Count);
        Assert.Contains(t.Placements, p => p == new ChildPlacement(0, 0, 0, 1, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(1, 0, 1, 1, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(2, 1, 0, 1, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(3, 1, 1, 1, 1));
    }

    [Fact]
    public void Column_spanning()
    {
        var result = BlueprintParser.Parse("0 0 1\n2 2 2");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Equal(2, t.Rows);
        Assert.Equal(3, t.Columns);
        Assert.Contains(t.Placements, p => p == new ChildPlacement(0, 0, 0, 1, 2));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(1, 0, 2, 1, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(2, 1, 0, 1, 3));
    }

    [Fact]
    public void Row_spanning()
    {
        var result = BlueprintParser.Parse("0 1\n0 2");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Contains(t.Placements, p => p == new ChildPlacement(0, 0, 0, 2, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(1, 0, 1, 1, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(2, 1, 1, 1, 1));
    }

    [Fact]
    public void Row_and_column_spanning()
    {
        var result = BlueprintParser.Parse(
            "0 1 1 2\n" +
            "0 1 1 2\n" +
            "0 1 1 3\n" +
            "4 4 4 4");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Equal(4, t.Rows);
        Assert.Equal(4, t.Columns);
        Assert.Contains(t.Placements, p => p == new ChildPlacement(0, 0, 0, 3, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(1, 0, 1, 3, 2));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(2, 0, 3, 2, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(3, 2, 3, 1, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(4, 3, 0, 1, 4));
    }

    [Fact]
    public void Slash_separator()
    {
        var result = BlueprintParser.Parse("0 1 / 2 3");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Equal(2, t.Rows);
        Assert.Equal(2, t.Columns);
        Assert.Equal(4, t.Placements.Count);
    }

    [Fact]
    public void Empty_cells_with_dot()
    {
        var result = BlueprintParser.Parse("0 . 1\n. . .\n2 . 3");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Equal(3, t.Rows);
        Assert.Equal(3, t.Columns);
        Assert.Equal(4, t.Placements.Count);
        Assert.Contains(t.Placements, p => p == new ChildPlacement(0, 0, 0, 1, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(1, 0, 2, 1, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(2, 2, 0, 1, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(3, 2, 2, 1, 1));
    }

    [Fact]
    public void Multi_digit_numbers()
    {
        var result = BlueprintParser.Parse("10 11 11 12");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Equal(1, t.Rows);
        Assert.Equal(4, t.Columns);
        Assert.Contains(t.Placements, p => p == new ChildPlacement(10, 0, 0, 1, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(11, 0, 1, 1, 2));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(12, 0, 3, 1, 1));
    }

    [Fact]
    public void Single_cell()
    {
        var result = BlueprintParser.Parse("0");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Equal(1, t.Rows);
        Assert.Equal(1, t.Columns);
        Assert.Single(t.Placements);
        Assert.Contains(t.Placements, p => p == new ChildPlacement(0, 0, 0, 1, 1));
    }

    [Fact]
    public void All_empty_cells()
    {
        var result = BlueprintParser.Parse(". .\n. .");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Equal(2, t.Rows);
        Assert.Equal(2, t.Columns);
        Assert.Empty(t.Placements);
    }

    // --- Validation error cases ---

    [Fact]
    public void Null_input_fails()
    {
        var result = BlueprintParser.Parse(null);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Empty_string_fails()
    {
        var result = BlueprintParser.Parse("");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Whitespace_only_fails()
    {
        var result = BlueprintParser.Parse("   \n  ");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Mismatched_row_widths_fails()
    {
        var result = BlueprintParser.Parse("0 1\n2 3 4");
        Assert.True(result.IsFailure);
        Assert.Contains("columns", result.Error);
    }

    [Fact]
    public void Invalid_token_fails()
    {
        var result = BlueprintParser.Parse("0 abc 1");
        Assert.True(result.IsFailure);
        Assert.Contains("abc", result.Error);
    }

    [Fact]
    public void Negative_number_fails()
    {
        var result = BlueprintParser.Parse("0 -1 2");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void L_shaped_region_fails()
    {
        // Child 0 forms an L-shape, not a rectangle:
        // 0 0
        // 0 1
        // 0 1  <-- child 0 is in col 0 rows 0-2, but also col 1 row 0 → L-shape
        var result = BlueprintParser.Parse("0 0\n0 1\n0 1");

        // This is actually a valid rectangle: child 0 is at rows 0-2, col 0. Let me create a real L-shape.
        var resultL = BlueprintParser.Parse("0 0 1\n0 . 1\n. 0 0");
        Assert.True(resultL.IsFailure);
        Assert.Contains("rectangle", resultL.Error);
    }

    [Fact]
    public void Disjoint_same_index_fails()
    {
        // Child 0 appears in non-contiguous cells
        var result = BlueprintParser.Parse("0 1 0");
        // The bounding box would be (0,0)→(0,2) = 3 cells, but only 2 are filled
        Assert.True(result.IsFailure);
        Assert.Contains("rectangle", result.Error);
    }

    [Fact]
    public void Mixed_slash_and_newline_separators()
    {
        var result = BlueprintParser.Parse("0 1 / 0 1\n2 2");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Equal(3, t.Rows);
        Assert.Equal(2, t.Columns);
        Assert.Contains(t.Placements, p => p == new ChildPlacement(0, 0, 0, 2, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(1, 0, 1, 2, 1));
        Assert.Contains(t.Placements, p => p == new ChildPlacement(2, 2, 0, 1, 2));
    }

    [Fact]
    public void Leading_and_trailing_whitespace_is_trimmed()
    {
        var result = BlueprintParser.Parse("  \n  0 1  \n  2 3  \n  ");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Equal(2, t.Rows);
        Assert.Equal(2, t.Columns);
    }

    [Fact]
    public void Placements_are_ordered_by_child_index()
    {
        var result = BlueprintParser.Parse("3 2\n1 0");

        Assert.True(result.IsSuccess);
        var t = result.Value;
        Assert.Equal(0, t.Placements[0].ChildIndex);
        Assert.Equal(1, t.Placements[1].ChildIndex);
        Assert.Equal(2, t.Placements[2].ChildIndex);
        Assert.Equal(3, t.Placements[3].ChildIndex);
    }
}