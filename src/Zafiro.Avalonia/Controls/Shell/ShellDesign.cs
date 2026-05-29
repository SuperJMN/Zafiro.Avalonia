using Zafiro.UI.Navigation.Sections;
using Zafiro.UI.Shell;

namespace Zafiro.Avalonia.Controls.Shell;

public class ShellDesign : IHierarchicalShell
{
    public ShellDesign()
    {
        var home = new SimpleSection { Id = "Home", FriendlyName = "Home", Icon = new Icon { Source = "fa-home" } };
        var settings = new SimpleSection { Id = "Settings", FriendlyName = "Settings", Icon = new Icon { Source = "fa-gear" } };
        var profile = new SimpleSection { Id = "Profile", ParentId = "Settings", FriendlyName = "Profile", Icon = new Icon { Source = "fa-user" } };
        var preferences = new SimpleSection { Id = "Preferences", ParentId = "Settings", FriendlyName = "Preferences", Icon = new Icon { Source = "fa-sliders" } };
        var sections = new ISection[] { home, settings, profile, preferences };

        Sections = sections;
        SelectedSection = new global::Reactive.Bindings.ReactiveProperty<ISection>(profile);
        SelectedPath = new global::Reactive.Bindings.ReactiveProperty<IReadOnlyList<ISection>>([settings, profile]);
        RootLevel = new SectionLevel([home, settings], settings, section => SelectedSection.Value = section);
        ChildLevels = new global::Reactive.Bindings.ReactiveProperty<IReadOnlyList<SectionLevel>>([
            new SectionLevel([profile, preferences], profile, section => SelectedSection.Value = section),
        ]);
    }

    public void GoToSection(string sectionName)
    {
    }

    public IEnumerable<ISection> Sections { get; }
    public SectionLevel RootLevel { get; }
    public global::Reactive.Bindings.ReactiveProperty<IReadOnlyList<SectionLevel>> ChildLevels { get; }
    public global::Reactive.Bindings.ReactiveProperty<ISection> SelectedSection { get; }
    public global::Reactive.Bindings.ReactiveProperty<IReadOnlyList<ISection>> SelectedPath { get; }
}
