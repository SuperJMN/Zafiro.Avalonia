using Zafiro.UI.Shell.Utils;

namespace ZafiroShellTemplate.Sections;

[Section("funded", "fa-circle-check", 1, FriendlyName = "Funded", ParentId = "investor")]
public class FundedViewModel
{
    public string Title => "Funded";
    public string Description => "Review projects already backed by this investor account.";
}
