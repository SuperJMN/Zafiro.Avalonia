using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Zafiro.Avalonia.Controls.Shell;
using Zafiro.Avalonia.Icons;
using Zafiro.Avalonia.Misc;
using Zafiro.UI.Shell;

namespace ZafiroShellTemplate;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        IconControlProviderRegistry.Register(new OptrisIconControlProvider(), asDefault: true);

        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var services = new ServiceCollection();
        services.AddZafiroShell(logger: logger);
        services.AddAllSectionsFromAttributes(logger);

        var provider = services.BuildServiceProvider();
        var shell = provider.GetRequiredService<IShell>();

        this.Connect(() => new ShellView(), _ => shell, () => new Window { Title = "ZafiroShellTemplate", Width = 900, Height = 600 });

        base.OnFrameworkInitializationCompleted();
    }
}
