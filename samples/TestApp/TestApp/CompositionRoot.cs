using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Serilog;
using TestApp.Samples.Navigation;
using TestApp.Shell;
using Zafiro.Avalonia;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.Services;
using Zafiro.Avalonia.Storage;
using Zafiro.UI;
using Zafiro.UI.Navigation;
using Zafiro.UI.Shell;
using Zafiro.UI.Shell.Utils;

namespace TestApp;

public static class CompositionRoot
{
    public static MainViewModel Create(Control view)
    {
        ServiceCollection services = new();

        services.AddSingleton<IShell, Zafiro.UI.Shell.Shell>();
        services.AddSingleton(new ShellProperties("Avalonia.Zafiro Tookit", navigatorObj => CreateHeaderFromNavigator(navigatorObj)));
        services.AddSingleton(DialogService.Create());
        services.AddSingleton<ILauncherService, LauncherService>();

        services.AddSingleton<INotificationService>(new NotificationService());

        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        services.AddScoped<INavigator>(provider => new Navigator(provider, logger, RxApp.MainThreadScheduler));
        services.AddAllSectionsFromAttributes(logger);
        services.AddTransient<MainViewModel>();
        services.AddTransient<TargetViewModel>();
        services.AddSingleton<IFileSystemPicker>(_ => { return new AvaloniaFileSystemPicker(TopLevel.GetTopLevel(view).StorageProvider); });

        var serviceProvider = services.BuildServiceProvider();

        if (!Design.IsDesignMode)
        {
            Commands.Instance = ActivatorUtilities.CreateInstance<Commands>(serviceProvider);
        }

        return serviceProvider.GetRequiredService<MainViewModel>();
    }

    private static IObservable<object?> CreateHeaderFromNavigator(object navigatorObj)
    {
        var navigator = (INavigator)navigatorObj;
        return navigator.Content.Select(o =>
        {
            var type = o?.GetType();

            var s = type?.GetCustomAttribute<SectionAttribute>()?.Name;
            if (s != null)
            {
                return s;
            }

            if (type is null)
            {
                return "Unknown Section";
            }

            return GetSectionName(type);
        });
    }

    private static string GetSectionName(Type getType)
    {
        string sectionName = getType.Name.Replace("ViewModel", "");
        string formattedName = string.Concat(sectionName.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        return formattedName;
    }
}