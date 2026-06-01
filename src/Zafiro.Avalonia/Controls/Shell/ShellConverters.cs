using Avalonia.Data.Converters;
using Zafiro.UI.Navigation.Sections;
using Zafiro.UI.Shell;

namespace Zafiro.Avalonia.Controls.Shell;

public static class ShellConverters
{
    public static readonly FuncValueConverter<SplitViewDisplayMode, bool> IsOverlay = new(displayMode => displayMode == SplitViewDisplayMode.Overlay || displayMode == SplitViewDisplayMode.CompactOverlay);
    public static readonly FuncValueConverter<IReadOnlyCollection<ISection>?, bool> HasMultipleSections = new(sections => sections is { Count: > 1 });
    public static readonly FuncValueConverter<IReadOnlyList<SectionLevel>?, SectionLevel?> FirstChildLevel = new(levels => levels?.FirstOrDefault());
}
