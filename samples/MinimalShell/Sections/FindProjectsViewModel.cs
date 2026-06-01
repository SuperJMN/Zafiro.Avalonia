using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section("find-projects", "fa-search", 0, FriendlyName = "Find Projects", ParentId = "investor")]
public class FindProjectsViewModel
{
    public string Title => "Find Projects";
    public string Description => "Search investment opportunities by stage, sector, and traction.";
}
