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
    private static readonly SectionGroup MainGroup = new("main", "Main");
    private static readonly SectionGroup AdminGroup = new("admin", "Administration");

    public IEnumerable<ISection> GroupedSections { get; } = new List<ISection>
    {
        new ContentSectionDesign
        {
            Name = "Dashboard",
            FriendlyName = "Dashboard",
            Icon = new Icon("fa-tachometer-alt"),
            Group = MainGroup
        },
        new ContentSectionDesign
        {
            Name = "Analytics",
            FriendlyName = "Analytics",
            Icon = new Icon("fa-chart-line"),
            Group = MainGroup
        },
        new ContentSectionDesign
        {
            Name = "Users",
            FriendlyName = "Users",
            Icon = new Icon("fa-users"),
            Group = AdminGroup
        },
        new ContentSectionDesign
        {
            Name = "Settings",
            FriendlyName = "Settings",
            Icon = new Icon("fa-cog"),
            Group = AdminGroup
        }
    };
}
