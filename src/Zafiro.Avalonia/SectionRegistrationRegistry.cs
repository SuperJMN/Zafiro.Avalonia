using System.Reactive.Concurrency;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Zafiro.UI.Shell.Utils;

public static class SectionRegistrationRegistry
{
    private static readonly object Sync = new();
    private static readonly Dictionary<string, Registration> Registrations = new();

    public static void Register(string assemblyName, Action<IServiceCollection> registerAnnotatedSections, Action<IServiceCollection, ILogger?, IScheduler?> addAnnotatedSections)
    {
        ArgumentNullException.ThrowIfNull(assemblyName);
        ArgumentNullException.ThrowIfNull(registerAnnotatedSections);
        ArgumentNullException.ThrowIfNull(addAnnotatedSections);

        lock (Sync)
        {
            if (Registrations.ContainsKey(assemblyName))
            {
                return;
            }

            Registrations[assemblyName] = new Registration(registerAnnotatedSections, addAnnotatedSections);
        }
    }

    public static IServiceCollection AddAllSectionsFromRegistry(this IServiceCollection services, ILogger? logger = null, IScheduler? scheduler = null)
    {
        scheduler ??= RxApp.MainThreadScheduler;

        Registration[] snapshot;
        lock (Sync)
        {
            snapshot = Registrations.Values.ToArray();
        }

        foreach (var registration in snapshot)
        {
            registration.Register(services);
        }

        foreach (var registration in snapshot)
        {
            registration.Add(services, logger, scheduler);
        }

        return services;
    }

    private sealed record Registration(Action<IServiceCollection> Register, Action<IServiceCollection, ILogger?, IScheduler?> Add);
}