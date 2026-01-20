using Avalonia.Controls.Notifications;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Services;

[PublicAPI]
public class NotificationService : INotificationService
{
    private readonly Lazy<WindowNotificationManager> manager;

    public NotificationService(NotificationPosition position = NotificationPosition.BottomRight)
    {
        manager = new Lazy<WindowNotificationManager>(() =>
        {
            var topLevel = ApplicationUtils.TopLevel().GetValueOrThrow("TopLevel not ready for NotificationService");
            var wnm = new WindowNotificationManager(topLevel) { Position = position };
            // WindowNotificationManager stores notifications in _items, which is initially a disconnected List.
            // Only after OnApplyTemplate() does _items point to the actual visual Panel.Children.
            // Without this call, notifications are added to the orphan list and never displayed.
            wnm.ApplyTemplate();
            return wnm;
        });
    }

    public Task Show(string message, Maybe<string> title)
    {
        Action action = () => manager.Value.Show(new Notification(title.GetValueOrDefault(), message));
        action.ExecuteOnUIThread();
        return Task.CompletedTask;
    }
}