using ReactiveUI.SourceGenerators;
using Zafiro.UI.Navigation;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls.Shell;

public partial class SimpleSection : ReactiveObject, INavigationRoot
{
    [Reactive] private object? icon;
    [Reactive] private bool isVisible = true;
    [Reactive] private INavigator navigator;
    [Reactive] private int sortOrder;

    public SimpleSection(object content)
    {
        Navigator = new SimpleNavigator(content);
    }

    public string Name { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public SectionGroup Group { get; set; } = new();
}