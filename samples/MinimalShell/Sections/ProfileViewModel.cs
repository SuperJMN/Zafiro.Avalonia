using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section(icon: "fa-user", sortIndex: 0, ParentId = "Settings")]
public class ProfileViewModel
{
    public string UserName => "Jane Doe";
    public string Email => "jane.doe@example.com";
}
