using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Zafiro.Avalonia.Controls;

namespace Zafiro.Avalonia.Tests;

public class EmptyItemsControlTests
{
    [AvaloniaFact]
    public void Items_control_gets_empty_class_when_no_items()
    {
        var itemsControl = new ItemsControl();
        Empty.SetContent(itemsControl, "Nothing");

        Assert.Contains(":empty", itemsControl.Classes);
    }

    [AvaloniaFact]
    public void Items_control_removes_empty_class_when_item_added()
    {
        var itemsControl = new ItemsControl();
        Empty.SetContent(itemsControl, "Nothing");

        itemsControl.Items.Add("item");

        Assert.DoesNotContain(":empty", itemsControl.Classes);
    }
}
