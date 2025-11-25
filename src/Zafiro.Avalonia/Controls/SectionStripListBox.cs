using Avalonia.Controls;

namespace Zafiro.Avalonia.Controls;

/// <summary>
/// A ListBox that creates <see cref="SectionStripItem"/> containers instead of <see cref="ListBoxItem"/>.
/// This allows users to style items using SectionStripItem selectors.
/// </summary>
public class SectionStripListBox : ListBox
{
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new SectionStripItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not SectionStripItem;
    }
}
