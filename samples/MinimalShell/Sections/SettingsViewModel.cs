using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section(icon: "fa-gear", sortIndex: 1)]
public class SettingsViewModel
{
    public string Description => "Configure your application preferences here.";
}
