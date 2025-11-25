using Avalonia;
using Avalonia.Markup.Xaml;
using TestApp.Shell;
using Zafiro.Avalonia.Icons;
using Zafiro.Avalonia.Misc;

namespace TestApp;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Register Svg-based icon provider for "svg:" sources
        IconControlProviderRegistry.Register(new SvgIconControlProvider());

        // Register Projektanker-based icon provider as the default for Zafiro.UI icons
        IconControlProviderRegistry.Register(new ProjektankerIconControlProvider(), asDefault: true);

        this.Connect(() => new MainView(), view => CompositionRoot.Create(view), () => new MainWindow());

        base.OnFrameworkInitializationCompleted();
    }
}