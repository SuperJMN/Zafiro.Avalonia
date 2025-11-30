using System.Collections.Generic;
using Zafiro.Avalonia.Controls.Shell;
using Zafiro.UI;
using Zafiro.UI.Navigation.Sections;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Shell;

[Section(name: "SectionStrip", icon: "fa-minus", sortIndex: 1)]
[SectionGroup("shell", "Shell")]
public class SectionStripViewModel
{
    private static readonly SectionGroup MainGroup = new("Main");
    private static readonly SectionGroup AdminGroup = new("Administration");

    public IEnumerable<ISection> GroupedSections { get; } = new List<ISection>
    {
        new SimpleSection()
        {
            Name = "Dashboard",
            FriendlyName = "Dashboard",
            Icon = new Icon("fa-tachometer-alt"),
            Group = MainGroup
        },
        new SimpleSection()
        {
            Name = "Analytics",
            FriendlyName = "Analytics",
            Icon = new Icon("fa-chart-line"),
            Group = MainGroup
        },
        new SimpleSection()
        {
            Name = "Users",
            FriendlyName = "Users",
            Icon = new Icon("fa-users"),
            Group = AdminGroup
        },
        new SimpleSection()
        {
            Name = "Settings",
            FriendlyName = "Settings",
            Icon = new Icon("fa-cog"),
            Group = AdminGroup
        }
    };
}