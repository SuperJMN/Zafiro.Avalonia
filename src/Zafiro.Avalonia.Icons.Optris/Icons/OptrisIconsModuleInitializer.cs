using System.Runtime.CompilerServices;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome;
using Optris.Icons.Avalonia.MaterialDesign;

namespace Zafiro.Avalonia.Icons;

/// <summary>
/// Module initializer that wires up the default Optris icon packs and
/// the path-based icon provider when the Zafiro.Avalonia.Icons.Optris
/// assembly is loaded.
/// </summary>
internal static class OptrisIconsModuleInitializer
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