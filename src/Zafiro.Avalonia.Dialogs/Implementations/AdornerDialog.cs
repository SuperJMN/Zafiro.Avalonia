using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using ReactiveUI;
using Zafiro.Avalonia.Dialogs.Views;

namespace Zafiro.Avalonia.Dialogs.Implementations;

public class AdornerDialog : IDialog, ICloseable
{
    private readonly Lazy<AdornerLayer> adornerLayerLazy;
    private readonly Stack<DialogEntry> dialogs = new();

    public AdornerDialog(Func<AdornerLayer> getAdornerLayer)
    {
        adornerLayerLazy = new Lazy<AdornerLayer>(() => getAdornerLayer());
    }

    public void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (dialogs.Count > 0)
            {
                var entry = dialogs.Pop();
                entry.TitleSubscription.Dispose();
                adornerLayerLazy.Value.Children.Remove(entry.Dialog);
                entry.Completion.TrySetResult(true);
            }
        });
    }

    public void Dismiss()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (dialogs.Count > 0)
            {
                var entry = dialogs.Pop();
                entry.TitleSubscription.Dispose();
                adornerLayerLazy.Value.Children.Remove(entry.Dialog);
                entry.Completion.TrySetResult(false);
            }
        });
    }

    public async Task<bool> Show<TViewModel>(TViewModel viewModel, IObservable<string> title, Func<TViewModel, ICloseable, IEnumerable<IOption>> optionsFactory)
    {
        if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
        if (title == null) throw new ArgumentNullException(nameof(title));
        if (optionsFactory == null) throw new ArgumentNullException(nameof(optionsFactory));

        var showTask = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var completion = new TaskCompletionSource<bool>();
            var options = optionsFactory(viewModel, this);

            var dialog = new DialogViewContainer
            {
                Content = new DialogControl()
                {
                    Content = viewModel,
                    Options = options,
                },
                Close = ReactiveCommand.Create(() => Dismiss()),
            };

            // Update the title reactively while the dialog is visible
            var titleSubscription = title
                .Subscribe(t => Dispatcher.UIThread.Post(() => dialog.Title = t ?? string.Empty));

            var adornerLayer = adornerLayerLazy.Value;

            dialog[!Layoutable.HeightProperty] = adornerLayer.Parent!
                .GetObservable(Visual.BoundsProperty)
                .Select(rect => rect.Height)
                .ToBinding();

            dialog[!Layoutable.WidthProperty] = adornerLayer.Parent!
                .GetObservable(Visual.BoundsProperty)
                .Select(rect => rect.Width)
                .ToBinding();

            adornerLayer.Children.Add(dialog);
            dialogs.Push(new DialogEntry(dialog, titleSubscription, completion));

            return completion.Task;
        });

        return showTask;
    }

    private sealed record DialogEntry(DialogViewContainer Dialog, IDisposable TitleSubscription, TaskCompletionSource<bool> Completion);
}