using Avalonia.Input;
using Avalonia.VisualTree;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Controls;

/// <summary>
/// A ListBox that creates <see cref="SectionStripItem"/> containers instead of <see cref="ListBoxItem"/>.
/// This allows users to style items using SectionStripItem selectors.
/// </summary>
/// <remarks>
/// When keyboard focus enters the strip from outside, it lands on the selected section rather than on whichever
/// section happens to be closest, as navigation bars on TVs and phones do.
/// </remarks>
public class SectionStripListBox : ListBox
{
    public SectionStripListBox()
    {
        AddHandler(GettingFocusEvent, OnItemGettingFocus);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new SectionStripItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not SectionStripItem;
    }

    private void OnItemGettingFocus(object? sender, FocusChangingEventArgs e)
    {
        if (e.NavigationMethod != NavigationMethod.Directional || SelectedIndex < 0)
        {
            return;
        }

        Visual strip = (Visual?)this.FindAncestorOfType<SectionStrip>() ?? this;
        if (KeyboardFocus.IsWithin(e.OldFocusedElement, strip) || !KeyboardFocus.IsWithin(e.NewFocusedElement, this))
        {
            return;
        }

        var selected = ContainerFromIndex(SelectedIndex);
        if (selected is not null && !ReferenceEquals(selected, e.NewFocusedElement) && KeyboardFocus.CanFocus(selected))
        {
            e.TrySetNewFocusedElement(selected);
        }
    }
}
