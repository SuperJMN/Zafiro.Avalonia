using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Dialogs.Views;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Dialogs.Implementations;

public class DesktopDialog : IDialog
{
    public async Task<bool> Show<TViewModel>(Maybe<TViewModel> viewModel, Maybe<IObservable<string>> title, Func<Maybe<TViewModel>, ICloseable, IEnumerable<IOption>> optionsFactory, Maybe<object> icon = default, DialogTone tone = DialogTone.Neutral, DialogSize size = DialogSize.Auto)
    {
        if (optionsFactory == null) throw new ArgumentNullException(nameof(optionsFactory));

        var showTask = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var mainWindow = ApplicationUtils.MainWindow().GetValueOrThrow("Cannot get the main window");

            var sizeHint = DialogSizeCalculator.Resolve(size);

            var window = new Window
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Icon = mainWindow.Icon,
                SizeToContent = SizeToContent.WidthAndHeight,
            };

            var screen = window.Screens.Primary ?? window.Screens.All.FirstOrDefault();
            double availW = 1280, availH = 720;
            if (screen != null)
            {
                var scaling = screen.Scaling;
                availW = screen.WorkingArea.Width / scaling;
                availH = screen.WorkingArea.Height / scaling;
            }

            var (minW, maxW, maxH) = DialogSizeCalculator.Calculate(sizeHint, availW, availH);
            window.MinWidth = minW;
            window.MaxWidth = maxW;
            window.MaxHeight = maxH;

            using var titleSubscription = title.GetValueOrDefault(Observable.Never<string>())
                .Subscribe(t => Dispatcher.UIThread.Post(() => window.Title = t ?? string.Empty));

            var closeable = new CloseableWrapper(window);
            var options = optionsFactory(viewModel, closeable);

            window.Content = new DialogControl
            {
                Content = viewModel.GetValueOrDefault(),
                Options = options,
                Icon = icon.GetValueOrDefault(),
                Tone = tone,
                SizeHint = sizeHint
            };

            var result = await window.ShowDialog<bool?>(mainWindow).ConfigureAwait(false);
            return result is not (null or false);
        });

        return showTask;
    }
}