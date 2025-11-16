using Avalonia.Controls;
using ProjektankerIcon = Projektanker.Icons.Avalonia.Icon;

namespace Zafiro.Avalonia.Icons;

/// <summary>
/// Default provider that renders <see cref="Zafiro.UI.IIcon"/> using Projektanker.Icons.Avalonia.Icon.
/// Intended to be registered as the default provider to keep backwards compatibility when desired.
/// </summary>
public class ProjektankerIconControlProvider : IIconControlProvider
{
    // Prefix is not normally used for routing because this provider is expected
    // to be used as the default provider, but it must be non-empty.
    public string Prefix => "projektanker";

    public Control? Create(Zafiro.UI.IIcon icon, string valueWithoutPrefix)
    {
        var source = icon.Source;
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        return new ProjektankerIcon { Value = source };
    }
}
