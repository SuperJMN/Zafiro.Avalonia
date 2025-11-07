using System.Runtime.CompilerServices;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia.MaterialDesign;

namespace Zafiro.Avalonia;

internal static class IconProviderBootstrapper
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>()
            .Register<MaterialDesignIconProvider>();
    }
}
