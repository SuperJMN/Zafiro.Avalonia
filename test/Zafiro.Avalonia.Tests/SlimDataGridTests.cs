using Avalonia.Headless.XUnit;
using Zafiro.Avalonia.Controls.SlimDataGrid;

namespace Zafiro.Avalonia.Tests;

public class SlimDataGridTests
{
    [AvaloniaFact]
    public void Show_headers_defaults_to_true_and_can_be_disabled()
    {
        var grid = new SlimDataGrid();

        Assert.True(grid.ShowHeaders);

        grid.ShowHeaders = false;

        Assert.False(grid.ShowHeaders);
    }
}
