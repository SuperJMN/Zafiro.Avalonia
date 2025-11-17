using Avalonia.Data.Converters;

namespace Zafiro.Avalonia.Controls.Shell;

public static class ShellConverters
{
    public static readonly FuncValueConverter<SplitViewDisplayMode, bool> IsOverlay = new(displayMode => displayMode == SplitViewDisplayMode.Overlay || displayMode == SplitViewDisplayMode.CompactOverlay);
}