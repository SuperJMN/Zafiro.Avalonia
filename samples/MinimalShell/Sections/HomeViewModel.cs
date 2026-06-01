using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section("home", "fa-home", 0, FriendlyName = "Home")]
public class HomeViewModel
{
    public string Greeting => "Welcome to the investment workspace.";
}
