using Projektanker.Icons.Avalonia;

namespace Zafiro.Avalonia.Icons;

public static class IconProviderExtensions
{
    public static IIconProviderContainer RegisterPathStringIconProvider(
        this IIconProviderContainer builder,
        string prefix)
    {
        var provider = new PathStringIconProvider(prefix);
        return builder.Register(provider);
    }
}