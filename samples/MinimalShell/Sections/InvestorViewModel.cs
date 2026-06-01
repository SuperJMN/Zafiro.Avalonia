using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section("investor", "fa-user", 2, FriendlyName = "Investor")]
public class InvestorViewModel
{
    public string Title => "Investor";
    public string Description => "Find projects and keep track of funded opportunities.";
}
