using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section(icon: "fa-home", sortIndex: 0)]
public class HomeViewModel
{
    public string Greeting => "Welcome to the Zafiro Shell!";
}
