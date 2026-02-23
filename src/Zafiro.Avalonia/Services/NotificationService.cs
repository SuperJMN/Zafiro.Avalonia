using System.Collections.Concurrent;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Services;

[PublicAPI]
public class NotificationService : INotificationService
{
    private readonly ConcurrentDictionary<TopLevel, WindowNotificationManager> managers = new();
    private readonly NotificationPosition position;

    public NotificationService(NotificationPosition position = NotificationPosition.BottomRight)
    {
        this.position = position;
    }

    public Task Show(string message, Maybe<string> title)
    {
        Action action = () =>
        {
            var topLevel = ApplicationUtils.TopLevel().GetValueOrDefault();
            if (topLevel == null) return;

            var wnm = managers.GetOrAdd(topLevel, CreateManager);
            EnsureOnTop(wnm);
            wnm.Show(new Notification(title.GetValueOrDefault(), message));
        };

        action.ExecuteOnUIThread();
        return Task.CompletedTask;
    }

    private WindowNotificationManager CreateManager(TopLevel topLevel)
    {
        var wnm = new WindowNotificationManager(topLevel) { Position = position };
        // WindowNotificationManager stores notifications in _items, which is initially a disconnected List.
        // Only after OnApplyTemplate() does _items point to the actual visual Panel.Children.
        // Without this call, notifications are added to the orphan list and never displayed.
        wnm.ApplyTemplate();
        return wnm;
    }

    private void EnsureOnTop(WindowNotificationManager wnm)
    {
        if (wnm.Parent is AdornerLayer adornerLayer)
        {
            var index = adornerLayer.Children.IndexOf(wnm);
            if (index >= 0 && index < adornerLayer.Children.Count - 1)
            {
                adornerLayer.Children.RemoveAt(index);
                adornerLayer.Children.Add(wnm);
            }
        }
    }
}