using System;
using System.Diagnostics;
using Avalonia;
using ReactiveUI.Avalonia;
using Zafiro.Avalonia.Mcp.AppHost;

namespace ZafiroShellTemplate.Desktop;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseMcpDiagnosticsIfDebug()
            .UseReactiveUI(_ => { });
}

internal static class AppBuilderMcpExtensions
{
    public static AppBuilder UseMcpDiagnosticsIfDebug(this AppBuilder builder)
    {
        var debugAttribute = (DebuggableAttribute?)Attribute.GetCustomAttribute(typeof(Program).Assembly, typeof(DebuggableAttribute));
        return debugAttribute?.IsJITTrackingEnabled is true ? builder.UseMcpDiagnostics() : builder;
    }
}
