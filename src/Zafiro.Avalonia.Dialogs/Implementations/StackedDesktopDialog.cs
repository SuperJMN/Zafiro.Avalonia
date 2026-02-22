using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Dialogs.Views;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Dialogs.Implementations;

public class StackedDesktopDialog : IDialog
{
    private static Window? dialogWindow;
    private static readonly Stack<DialogContext> DialogStack = new();
    private static IDisposable? titleSubscription;

    public async Task<bool> Show<TViewModel>(Maybe<TViewModel> viewModel, Maybe<IObservable<string>> title, Func<Maybe<TViewModel>, ICloseable, IEnumerable<IOption>> optionsFactory, Maybe<object> icon = default, DialogTone tone = DialogTone.Neutral)
    {
        if (optionsFactory == null) throw new ArgumentNullException(nameof(optionsFactory));

        var showTask = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var mainWindow = ApplicationUtils.MainWindow().GetValueOrThrow("Cannot get the main window");

            var completionSource = new TaskCompletionSource<bool>();
            var closeable = new DialogCloseable(completionSource, true);
            var options = optionsFactory(viewModel, closeable).ToList();

            // Create a context instance for the current dialog
            var dialogContext = new DialogContext(viewModel.GetValueOrDefault(), title, options, completionSource, icon.GetValueOrDefault(), tone);

            // Add the dialog to the stack
            DialogStack.Push(dialogContext);

            // If there is no dialog window, create a new one
            if (dialogWindow == null)
            {
                dialogWindow = new Window
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Icon = mainWindow.Icon,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    MaxWidth = 800,
                    MaxHeight = 700,
                    MinWidth = 400,
                    MinHeight = 300
                };

                // Handle the window closing event to complete every pending dialog
                dialogWindow.Closed += (sender, args) =>
                {
                    while (DialogStack.Count > 0)
                    {
                        var dialog = DialogStack.Pop();
                        dialog.CompletionSource.TrySetResult(false);
                    }

                    dialogWindow = null;
                };

                // Update the content with the current dialog
                UpdateDialogContent(dialogContext);

                // Show the dialog window
                dialogWindow.Show(mainWindow);
            }
            else
            {
                // If there is already a dialog window, update its content
                UpdateDialogContent(dialogContext);
            }

            // Wait for the current dialog to complete
            return completionSource.Task;
        });

        return showTask;
    }

    private static void UpdateDialogContent(DialogContext dialogContext)
    {
        if (dialogWindow != null)
        {
            dialogWindow.Title = string.Empty;
            titleSubscription?.Dispose();

            titleSubscription = dialogContext.Title.GetValueOrDefault(Observable.Never<string>())
                .Subscribe(t => Dispatcher.UIThread.Post(() =>
                {
                    if (dialogWindow != null)
                    {
                        dialogWindow.Title = t ?? string.Empty;
                    }
                }));

            dialogWindow.Content = new DialogControl
            {
                Content = dialogContext.ViewModel,
                Options = dialogContext.Options,
                Icon = dialogContext.Icon,
                Tone = dialogContext.Tone
            };
        }
    }

    private class DialogContext
    {
        public DialogContext(object? viewModel, Maybe<IObservable<string>> title, IEnumerable<IOption> options, TaskCompletionSource<bool> completionSource, object? icon, DialogTone tone)
        {
            ViewModel = viewModel;
            Title = title;
            Options = options;
            CompletionSource = completionSource;
            Icon = icon;
            Tone = tone;
        }

        public object? ViewModel { get; }
        public object? Icon { get; }
        public DialogTone Tone { get; }
        public Maybe<IObservable<string>> Title { get; }
        public IEnumerable<IOption> Options { get; }
        public TaskCompletionSource<bool> CompletionSource { get; }
    }

    private class DialogCloseable : ICloseable
    {
        private readonly TaskCompletionSource<bool> completionSource;
        private readonly bool result;

        public DialogCloseable(TaskCompletionSource<bool> completionSource, bool result)
        {
            this.completionSource = completionSource;
            this.result = result;
        }

        public void Close()
        {
            Dispatcher.UIThread.Post(() =>
            {
                // Complete the current dialog with the corresponding result
                completionSource.TrySetResult(result);

                // Remove the current dialog from the stack
                if (DialogStack.Count > 0)
                {
                    DialogStack.Pop();
                }

                // If there are more dialogs on the stack, show the next one
                if (DialogStack.Count > 0)
                {
                    var nextDialog = DialogStack.Peek();
                    UpdateDialogContent(nextDialog);
                }
                else
                {
                    // If there are no more dialogs, close the window
                    dialogWindow?.Close();
                    dialogWindow = null;
                }
            });
        }

        public void Dismiss()
        {
            Dispatcher.UIThread.Post(() =>
            {
                // Complete the current dialog with a false result (canceled/dismissed)
                completionSource.TrySetResult(false);

                // Remove the current dialog from the stack
                if (DialogStack.Count > 0)
                {
                    DialogStack.Pop();
                }

                // If there are more dialogs on the stack, show the next one
                if (DialogStack.Count > 0)
                {
                    var nextDialog = DialogStack.Peek();
                    UpdateDialogContent(nextDialog);
                }
                else
                {
                    // If there are no more dialogs, close the window
                    dialogWindow?.Close();
                    dialogWindow = null;
                }
            });
        }
    }
}