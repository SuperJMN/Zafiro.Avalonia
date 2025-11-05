using Avalonia.Data.Converters;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls.Shell;

public static class ShellConverters
{
    public static readonly FuncValueConverter<ISection, bool> IsActivatable = new(sectionBase => { return sectionBase is not ISectionSeparator; });

    public static readonly FuncValueConverter<SplitViewDisplayMode, bool> IsOverlay = new(displayMode => displayMode == SplitViewDisplayMode.Overlay || displayMode == SplitViewDisplayMode.CompactOverlay);
}
