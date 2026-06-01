using Zafiro.UI.Shell.Utils;

namespace ZafiroShellTemplate.Sections;

[Section("home", "fa-home", 0, FriendlyName = "Home")]
public class HomeViewModel
{
    public string Greeting => "Welcome to your investment workspace.";
}
