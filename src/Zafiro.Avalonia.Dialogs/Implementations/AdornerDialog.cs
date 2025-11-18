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

    private TaskCompletionSource<bool>? currentDialog;

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
            }

            currentDialog?.TrySetResult(true);
            currentDialog = null;
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
            }

            currentDialog?.TrySetResult(false);
            currentDialog = null;
        });
    }

    public async Task<bool> Show(object viewModel, IObservable<string> title, Func<ICloseable, IEnumerable<IOption>> optionsFactory)
    {
        if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
        if (title == null) throw new ArgumentNullException(nameof(title));
        if (optionsFactory == null) throw new ArgumentNullException(nameof(optionsFactory));

        var showTask = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            currentDialog = new TaskCompletionSource<bool>();
            var options = optionsFactory(this);

            var dialog = new DialogViewContainer
            {
                Content = new DialogControl()
                {
                    Content = viewModel,
                    Options = options,
                },
                Close = ReactiveCommand.Create(() => Dismiss()),
            };

            // Actualizar título de forma reactiva mientras el diálogo está visible
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
            dialogs.Push(new DialogEntry(dialog, titleSubscription));

            return currentDialog.Task;
        });

        return showTask;
    }

    private sealed record DialogEntry(DialogViewContainer Dialog, IDisposable TitleSubscription);
}