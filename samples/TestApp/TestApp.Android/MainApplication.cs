using System;
using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using ReactiveUI.Avalonia;
using Zafiro.Avalonia.Mcp.AppHost;

namespace TestApp.Android;

[Application]
public class MainApplication : AvaloniaAndroidApplication<App>
{
    public MainApplication(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseMcpDiagnostics()
            .UseReactiveUI(_ => { });
    }
}
