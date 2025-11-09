using Avalonia.Controls.Notifications;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Services;

[PublicAPI]
public class NotificationService : INotificationService
{
    private readonly Lazy<IManagedNotificationManager> manager;

    public NotificationService(Func<IManagedNotificationManager> factory)
    {
        manager = new Lazy<IManagedNotificationManager>(factory);
    }

    public Task Show(string message, Maybe<string> title)
    {
        Action action = () => manager.Value.Show(new Notification(title.GetValueOrDefault(), message));
        action.ExecuteOnUIThread();
        return Task.CompletedTask;
    }
}