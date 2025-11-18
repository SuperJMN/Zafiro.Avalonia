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

            // Crea una instancia de contexto para el diálogo actual
            var dialogContext = new DialogContext(viewModel, title, options, completionSource);

            // Añade el diálogo a la pila
            DialogStack.Push(dialogContext);

            // Si no hay ventana de diálogo, crea una nueva
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

                // Maneja el evento de cierre de la ventana para completar todos los diálogos pendientes
                dialogWindow.Closed += (sender, args) =>
                {
                    while (DialogStack.Count > 0)
                    {
                        var dialog = DialogStack.Pop();
                        dialog.CompletionSource.TrySetResult(false);
                    }

                    dialogWindow = null;
                };

                // Actualiza el contenido con el diálogo actual
                UpdateDialogContent(dialogContext);

                // Muestra la ventana de diálogo
                dialogWindow.Show(mainWindow);
            }
            else
            {
                // Si ya hay una ventana de diálogo, actualiza su contenido
                UpdateDialogContent(dialogContext);
            }

            // Espera a que se complete el diálogo actual
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
                // Completa el diálogo actual con el resultado correspondiente
                completionSource.TrySetResult(result);

                // Quita el diálogo actual de la pila
                if (DialogStack.Count > 0)
                {
                    DialogStack.Pop();
                }

                // Si hay más diálogos en la pila, muestra el siguiente
                if (DialogStack.Count > 0)
                {
                    var nextDialog = DialogStack.Peek();
                    UpdateDialogContent(nextDialog);
                }
                else
                {
                    // Si no hay más diálogos, cierra la ventana
                    dialogWindow?.Close();
                    dialogWindow = null;
                }
            });
        }

        public void Dismiss()
        {
            Dispatcher.UIThread.Post(() =>
            {
                // Completa el diálogo actual con resultado falso (cancelado/descartado)
                completionSource.TrySetResult(false);

                // Quita el diálogo actual de la pila
                if (DialogStack.Count > 0)
                {
                    DialogStack.Pop();
                }

                // Si hay más diálogos en la pila, muestra el siguiente
                if (DialogStack.Count > 0)
                {
                    var nextDialog = DialogStack.Peek();
                    UpdateDialogContent(nextDialog);
                }
                else
                {
                    // Si no hay más diálogos, cierra la ventana
                    dialogWindow?.Close();
                    dialogWindow = null;
                }
            });
        }
    }
}