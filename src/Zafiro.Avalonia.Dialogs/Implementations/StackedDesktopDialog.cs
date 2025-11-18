using Avalonia.Controls;
using Avalonia.Threading;
using Zafiro.Avalonia.Dialogs.Views;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Dialogs.Implementations;

public class StackedDesktopDialog : IDialog
{
    private static Window? dialogWindow;
    private static readonly Stack<DialogContext> DialogStack = new();
    private static IDisposable? titleSubscription;

    public async Task<bool> Show(object viewModel, IObservable<string> title, Func<ICloseable, IEnumerable<IOption>> optionsFactory)
    {
        if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
        if (title == null) throw new ArgumentNullException(nameof(title));
        if (optionsFactory == null) throw new ArgumentNullException(nameof(optionsFactory));

        var showTask = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var mainWindow = ApplicationUtils.MainWindow().GetValueOrThrow("Cannot get the main window");

            var completionSource = new TaskCompletionSource<bool>();
            var closeable = new DialogCloseable(completionSource, true);
            var options = optionsFactory(closeable).ToList();

            // Create a context instance for the current dialog
            var dialogContext = new DialogContext(viewModel, title, options, completionSource);

            // Push the dialog onto the stack
            DialogStack.Push(dialogContext);

            // If there is no dialog window yet, create a new one
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

                // Handle the window close event to complete every pending dialog
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
                // If a dialog window already exists, just update its content
                UpdateDialogContent(dialogContext);
            }

            // Wait until the current dialog completes
            return completionSource.Task;
        });

        return showTask;
    }

    private static void UpdateDialogContent(DialogContext dialogContext)
    {
        if (dialogWindow != null)
        {
            titleSubscription?.Dispose();
            titleSubscription = dialogContext.Title
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
                Options = dialogContext.Options
            };
        }
    }

    private class DialogContext
    {
        public DialogContext(object viewModel, IObservable<string> title, IEnumerable<IOption> options, TaskCompletionSource<bool> completionSource)
        {
            ViewModel = viewModel;
            Title = title;
            Options = options;
            CompletionSource = completionSource;
        }

        public object ViewModel { get; }
        public IObservable<string> Title { get; }
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
                // Complete the current dialog with a false result (cancelled/dismissed)
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