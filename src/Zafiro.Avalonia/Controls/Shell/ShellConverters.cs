using Avalonia.Data.Converters;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls.Shell;

public static class ShellConverters
{
public static readonly FuncValueConverter<ISection, bool> IsActivatable = new(sectionBase => sectionBase is not ISectionSeparator && sectionBase is not ISectionGroupHeader);

    public static readonly FuncValueConverter<ISection, Dock> IsPrimaryToDock = new(section => section.IsPrimary ? Dock.Top : Dock.Bottom);
    public static readonly FuncValueConverter<SplitViewDisplayMode, bool> IsOverlay = new(displayMode => displayMode == SplitViewDisplayMode.Overlay || displayMode == SplitViewDisplayMode.CompactOverlay);
}