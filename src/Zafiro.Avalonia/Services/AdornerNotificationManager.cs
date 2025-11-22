using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;

namespace Zafiro.Avalonia.Services;

public class AdornerNotificationManager : IManagedNotificationManager
{
    private readonly Lazy<AdornerLayer> adornerLayer;
    private readonly Lazy<NotificationHost> host;

    public AdornerNotificationManager(Func<AdornerLayer> adornerLayerFactory)
    {
        adornerLayer = new Lazy<AdornerLayer>(adornerLayerFactory);
        host = new Lazy<NotificationHost>(CreateHost);
    }

    public void Show(INotification notification)
    {
        Dispatcher.UIThread.Post(() => host.Value.AddNotification(notification));
    }

    private NotificationHost CreateHost()
    {
        var panel = new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(12),
        };

        Panel.SetZIndex(panel, int.MaxValue);

        var layer = adornerLayer.Value;
        layer.Children.Add(panel);

        return new NotificationHost(panel, layer);
    }

    private sealed record NotificationHost(StackPanel Panel, AdornerLayer Layer)
    {
        public void AddNotification(INotification notification)
        {
            var card = new NotificationCard
            {
                Notification = notification,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            card.Closed += (_, _) => Remove(card);

            Panel.Children.Add(card);

            var expiration = notification.Expiration ?? TimeSpan.FromSeconds(5);
            DispatcherTimer.RunOnce(() => Remove(card), expiration);
        }

        private void Remove(Control card)
        {
            Panel.Children.Remove(card);

            if (Panel.Children.Count == 0)
            {
                Layer.Children.Remove(Panel);
            }
        }
    }
}
