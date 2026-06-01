using Zafiro.UI.Navigation.Sections;
using Zafiro.UI.Shell;

namespace Zafiro.Avalonia.Controls.Shell;

public class ShellDesign : IHierarchicalShell
{
    public ShellDesign()
    {
        var home = new SimpleSection { Id = "Home", FriendlyName = "Home", Icon = new Icon { Source = "fa-home" }, Content = "Home" };
        var funds = new SimpleSection { Id = "Funds", FriendlyName = "Funds", Icon = new Icon { Source = "fa-wallet" }, Content = "Funds" };
        var investor = new SimpleSection { Id = "Investor", FriendlyName = "Investor", Icon = new Icon { Source = "fa-user" }, Content = "Investor" };
        var findProjects = new SimpleSection { Id = "FindProjects", ParentId = "Investor", FriendlyName = "Find Projects", Icon = new Icon { Source = "fa-search" }, Content = "Find Projects" };
        var funded = new SimpleSection { Id = "Funded", ParentId = "Investor", FriendlyName = "Funded", Icon = new Icon { Source = "fa-circle-check" }, Content = "Funded" };
        var founder = new SimpleSection { Id = "Founder", FriendlyName = "Founder", Icon = new Icon { Source = "fa-lightbulb" }, Content = "Founder" };
        var myProjects = new SimpleSection { Id = "MyProjects", ParentId = "Founder", FriendlyName = "My Projects", Icon = new Icon { Source = "fa-folder-open" }, Content = "My Projects" };
        var funders = new SimpleSection { Id = "Funders", ParentId = "Founder", FriendlyName = "Funders", Icon = new Icon { Source = "fa-handshake" }, Content = "Funders" };
        var sections = new ISection[] { home, funds, investor, findProjects, funded, founder, myProjects, funders };

        Sections = sections;
        SelectedSection = new global::Reactive.Bindings.ReactiveProperty<ISection>(findProjects);
        SelectedPath = new global::Reactive.Bindings.ReactiveProperty<IReadOnlyList<ISection>>([investor, findProjects]);
        RootLevel = new SectionLevel([home, funds, investor, founder], investor, section => SelectedSection.Value = section);
        ChildLevels = new global::Reactive.Bindings.ReactiveProperty<IReadOnlyList<SectionLevel>>([
            new SectionLevel([findProjects, funded], findProjects, section => SelectedSection.Value = section),
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
