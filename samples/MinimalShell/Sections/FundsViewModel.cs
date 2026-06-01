using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section("funds", "fa-wallet", 1, FriendlyName = "Funds")]
public class FundsViewModel
{
    public string Description => "Browse available funds and portfolio activity.";
}
