using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section(icon: "fa-user", sortIndex: 2)]
public class ProfileViewModel
{
    public string UserName => "Jane Doe";
    public string Email => "jane.doe@example.com";
}
