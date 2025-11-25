using Avalonia.Controls.Primitives;

namespace Zafiro.Avalonia.Controls;

/// <summary>
/// Represents a group header in a <see cref="SectionStrip"/>.
/// Provides a distinct type for styling section group headers.
/// </summary>
/// <remarks>
/// Users can style this control using selectors like:
/// <code>
/// Style Selector="SectionStripGroupHeader" - base style
/// </code>
/// Common properties to customize: Background, Foreground, FontWeight, FontSize, Padding, Margin, CornerRadius.
/// </remarks>
public class SectionStripGroupHeader : HeaderedContentControl;
