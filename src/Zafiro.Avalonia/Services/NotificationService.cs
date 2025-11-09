using Avalonia.Controls.Notifications;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Services;

[PublicAPI]
public class NotificationService : INotificationService
{
    private readonly Func<IManagedNotificationManager> factory;

    public NotificationService(Func<IManagedNotificationManager> factory)
    {
        this.factory = factory;
    }

    public Task Show(string message, Maybe<string> title)
    {
        Action action = () => factory().Show(new Notification(title.GetValueOrDefault(), message));
        action.ExecuteOnUIThread();
        return Task.CompletedTask;
    }
}