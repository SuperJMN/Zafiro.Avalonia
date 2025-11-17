using System.Runtime.CompilerServices;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia.MaterialDesign;

namespace Zafiro.Avalonia.Icons;

/// <summary>
/// Module initializer that wires up the default Projektanker icon packs and
/// the path-based icon provider when the Zafiro.Avalonia.Icons.Projektanker
/// assembly is loaded.
/// </summary>
internal static class ProjektankerIconsModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>()
            .Register<MaterialDesignIconProvider>()
            .Register<PathStringIconProvider>();
    }
}