using Zafiro.UI.Navigation.Sections;
using Zafiro.UI.Shell;

namespace Zafiro.Avalonia.Controls.Shell;

public class ShellDesign : IShell
{
    public ShellDesign()
    {
        var sections = new ISection[]
        {
            new SimpleSection { Id = "Home", FriendlyName = "Home", Icon = new Icon { Source = "fa-home" } },
            new SimpleSection { Id = "Settings", FriendlyName = "Settings", Icon = new Icon { Source = "fa-gear" } },
            new SimpleSection { Id = "Profile", FriendlyName = "Profile", Icon = new Icon { Source = "fa-user" } },
        };

        Sections = sections;
        SelectedSection = new global::Reactive.Bindings.ReactiveProperty<ISection>(sections[0]);
    }

    public void GoToSection(string sectionName)
    {
    }

    public IEnumerable<ISection> Sections { get; }
    public global::Reactive.Bindings.ReactiveProperty<ISection> SelectedSection { get; }
}