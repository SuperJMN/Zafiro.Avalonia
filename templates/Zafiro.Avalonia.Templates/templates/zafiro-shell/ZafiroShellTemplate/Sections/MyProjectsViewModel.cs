using Zafiro.UI.Shell.Utils;

namespace ZafiroShellTemplate.Sections;

[Section("my-projects", "fa-folder-open", 0, FriendlyName = "My Projects", ParentId = "founder")]
public class MyProjectsViewModel
{
    public string Title => "My Projects";
    public string Description => "Track drafts, live rounds, and project readiness.";
}
