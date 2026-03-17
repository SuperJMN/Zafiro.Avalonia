using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Serilog;
using TestApp.Samples;
using TestApp.Samples.Navigation;
using TestApp.Shell;
using Zafiro.Avalonia;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.Services;
using Zafiro.Avalonia.Storage;
using Zafiro.UI;
using Zafiro.UI.Navigation;
using Zafiro.UI.Shell.Utils;

namespace TestApp;

public static class CompositionRoot
{
    public static MainViewModel Create(Control view)
    {
        ServiceCollection services = new();

        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        // Register all section ViewModels from [Section] attributes via the source generator.
        services.AddAllSectionsFromAttributes(logger);

        services.AddSingleton(DialogService.Create());
        services.AddSingleton<ILauncherService, LauncherService>();
        services.AddSingleton<INotificationService>(new NotificationService());
        services.AddTransient<MainViewModel>();
        services.AddTransient<TargetViewModel>();
        services.AddSingleton<IFileSystemPicker>(_ => new AvaloniaFileSystemPicker(TopLevel.GetTopLevel(view).StorageProvider));

        // Build sample cards from [Section]/[SectionGroup] attributes.
        var cards = BuildSampleCards();
        services.AddSingleton<IEnumerable<SampleCard>>(cards);
        services.AddSingleton<HomeViewState>();
        services.AddTransient<HomeViewModel>();

        // Register INavigator as singleton (the hub uses a single navigator for the whole app).
        services.AddSingleton<INavigator>(sp =>
            new Navigator(sp, ((ILogger)logger).AsMaybe(), RxApp.MainThreadScheduler));

        var serviceProvider = services.BuildServiceProvider();

        if (!Design.IsDesignMode)
        {
            Commands.Instance = ActivatorUtilities.CreateInstance<Commands>(serviceProvider);
        }

        // Set the initial page to the Home hub.
        var navigator = serviceProvider.GetRequiredService<INavigator>();
        navigator.SetInitialPage(() => serviceProvider.GetRequiredService<HomeViewModel>());

        return serviceProvider.GetRequiredService<MainViewModel>();
    }

    private static List<SampleCard> BuildSampleCards()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var cards = new List<SampleCard>();

        foreach (var type in assembly.GetTypes())
        {
            var sectionAttr = type.GetCustomAttribute<SectionAttribute>();
            if (sectionAttr is null)
            {
                continue;
            }

            var baseName = type.Name.EndsWith("ViewModel", StringComparison.Ordinal)
                ? type.Name[..^"ViewModel".Length]
                : type.Name;

            var displayName = string.Concat(baseName.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart();
            var name = sectionAttr.Name ?? displayName;
            var icon = sectionAttr.Icon ?? "fa-window-maximize";

            var groupAttr = type.GetCustomAttribute<SectionGroupAttribute>();
            var category = groupAttr?.FriendlyName ?? groupAttr?.Key ?? "General";

            var contractType = sectionAttr.ContractType ?? type;

            cards.Add(new SampleCard(name, $"Demo of {name}", icon, category, contractType));
        }

        return cards;
    }
}