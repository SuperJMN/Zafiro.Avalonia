using Zafiro.UI.Shell.Utils;

namespace ZafiroShellTemplate.Sections;

[Section(icon: "fa-home", sortIndex: 0)]
public class HomeViewModel
{
    public string Greeting => "Welcome to your Zafiro Shell app!";
}
