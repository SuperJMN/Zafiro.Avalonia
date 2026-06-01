using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section("founder", "fa-lightbulb", 3, FriendlyName = "Founder")]
public class FounderViewModel
{
    public string Title => "Founder";
    public string Description => "Navigation group for founder workflows. Selecting it opens My Projects by default.";
}
