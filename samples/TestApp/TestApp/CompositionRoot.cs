using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TestApp.Samples.Navigation;
using TestApp.Shell;
using Zafiro.Avalonia;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.Services;
using Zafiro.Avalonia.Storage;
using Zafiro.UI;
using Zafiro.UI.Shell;

namespace TestApp;

public static class CompositionRoot
{
    public static MainViewModel Create(Control view)
    {
        ServiceCollection services = new();

        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        services.AddZafiroShell(logger: logger);
        services.AddAllSectionsFromAttributes(logger);

        services.AddSingleton(DialogService.Create());
        services.AddSingleton<ILauncherService, LauncherService>();
        services.AddSingleton<INotificationService>(new NotificationService());
        services.AddTransient<MainViewModel>();
        services.AddTransient<TargetViewModel>();
        services.AddSingleton<IFileSystemPicker>(_ => new AvaloniaFileSystemPicker(TopLevel.GetTopLevel(view).StorageProvider));

        var serviceProvider = services.BuildServiceProvider();

        if (!Design.IsDesignMode)
        {
            Commands.Instance = ActivatorUtilities.CreateInstance<Commands>(serviceProvider);
        }

        return serviceProvider.GetRequiredService<MainViewModel>();
    }
}