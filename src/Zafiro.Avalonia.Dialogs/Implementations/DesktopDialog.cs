using Avalonia.Controls;
using Avalonia.Threading;
using Zafiro.Avalonia.Dialogs.Views;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Dialogs.Implementations;

public class DesktopDialog : IDialog
{
    public async Task<bool> Show(object viewModel, IObservable<string> title, Func<ICloseable, IEnumerable<IOption>> optionsFactory)
    {
        if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
        if (title == null) throw new ArgumentNullException(nameof(title));
        if (optionsFactory == null) throw new ArgumentNullException(nameof(optionsFactory));

        var showTask = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var mainWindow = ApplicationUtils.MainWindow().GetValueOrThrow("Cannot get the main window");

            var window = new Window
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Icon = mainWindow.Icon,
                SizeToContent = SizeToContent.WidthAndHeight,
            };

            using var titleSubscription = title
                .Subscribe(t => Dispatcher.UIThread.Post(() => window.Title = t ?? string.Empty));

            var closeable = new CloseableWrapper(window);
            var options = optionsFactory(closeable);

            window.Content = new DialogControl
            {
                Content = viewModel,
                Options = options
            };

            var result = await window.ShowDialog<bool?>(mainWindow).ConfigureAwait(false);
            return result is not (null or false);
        });

        return showTask;
    }
}