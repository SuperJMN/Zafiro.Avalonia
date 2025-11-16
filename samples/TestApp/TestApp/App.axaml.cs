using Avalonia;
using Avalonia.Markup.Xaml;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia.MaterialDesign;
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
        // Register Projektanker-based icon provider as the default for Zafiro.UI icons
        IconControlProviderRegistry.Register(new ProjektankerIconControlProvider(), asDefault: true);

        IconProvider.Current
            .Register<FontAwesomeIconProvider>()
            .Register<MaterialDesignIconProvider>();

        this.Connect(() => new MainView(), view => CompositionRoot.Create(), () => new MainWindow());

        base.OnFrameworkInitializationCompleted();
    }
}