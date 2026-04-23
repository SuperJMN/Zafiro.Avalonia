using System.Runtime.CompilerServices;

namespace Zafiro.Avalonia.Icons;

/// <summary>
/// Module initializer that registers the icon providers shipped with the core
/// Zafiro.Avalonia assembly (no external dependencies).
/// </summary>
internal static class CoreIconsModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        IconControlProviderRegistry.Register(new TextIconControlProvider());
    }
}
