using Avalonia.Controls;
using Zafiro.Avalonia.Controls;
using Zafiro.UI;
using OptrisIcon = Optris.Icons.Avalonia.Icon;

namespace Zafiro.Avalonia.Icons;

/// <summary>
/// Default provider that renders <see cref="Zafiro.UI.IIcon"/> using Optris.Icons.Avalonia.Icon.
/// Intended to be registered as the default provider to keep backwards compatibility when desired.
/// </summary>
public class OptrisIconControlProvider : IIconControlProvider
{
    public string Prefix => "optris";

    public Control? Create(IIcon icon, string valueWithoutPrefix)
    {
        var source = icon.Source;
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        var optrisIcon = new OptrisIcon { Value = source };
        optrisIcon[!OptrisIcon.ForegroundProperty] = optrisIcon[!IconOptions.FillProperty];
        return optrisIcon;
    }
}