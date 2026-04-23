using System;
using Avalonia;
using ReactiveUI.Avalonia;
using Zafiro.Avalonia.Mcp.AppHost;

namespace MinimalShell;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseMcpDiagnostics()
            .WithInterFont()
#if DEBUG
            .WithDeveloperTools()
#endif
            .UseReactiveUI(_ => { });
}
