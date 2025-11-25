using Avalonia.Controls;

namespace Zafiro.Avalonia.Controls;

/// <summary>
/// Represents an item in a <see cref="SectionStrip"/>.
/// Inherits from <see cref="ListBoxItem"/> to maintain selection behavior
/// while providing a distinct type for styling purposes.
/// </summary>
/// <remarks>
/// Users can style this control using selectors like:
/// <code>
/// Style Selector="SectionStripItem" - base style
/// Style Selector="SectionStripItem:selected" - selected state
/// Style Selector="SectionStripItem:pointerover" - hover state
/// </code>
/// </remarks>
public class SectionStripItem : ListBoxItem;
