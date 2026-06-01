using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section("funders", "fa-handshake", 1, FriendlyName = "Funders", ParentId = "founder")]
public class FundersViewModel
{
    public string Title => "Funders";
    public string Description => "Review interested investors and manage outreach.";
}
