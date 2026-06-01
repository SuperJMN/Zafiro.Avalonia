using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls.Shell;

public sealed record SectionTreeNode(ISection Section, IReadOnlyList<SectionTreeNode> Children)
{
    public string Id => Section.Id;
    public string FriendlyName => Section.FriendlyName;
    public object? Icon => Section.Icon;
}
