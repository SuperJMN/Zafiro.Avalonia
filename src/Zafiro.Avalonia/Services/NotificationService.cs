using Avalonia.Controls.Notifications;
using Avalonia.Threading;
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

        // Force eager initialization on the UI thread to prevent the first notification from being lost.
        // This is a workaround for a known Avalonia issue where WindowNotificationManager 
        // cannot display notifications until its template is fully applied.
        // We use Post to defer until the next dispatcher cycle, when TopLevel will be ready.
        Dispatcher.UIThread.Post(() => _ = manager.Value, DispatcherPriority.Loaded);
    }

    public Task Show(string message, Maybe<string> title)
    {
        Action action = () => manager.Value.Show(new Notification(title.GetValueOrDefault(), message));
        action.ExecuteOnUIThread();
        return Task.CompletedTask;
    }
}