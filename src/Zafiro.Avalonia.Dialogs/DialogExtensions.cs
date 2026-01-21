using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using CSharpFunctionalExtensions;
using ReactiveUI;
using Zafiro.Avalonia.Dialogs.Views;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.UI;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Dialogs;

public static class DialogExtensions
{
    // Compatibility: classic API with a title as string
    public static Task<bool> Show(this IDialog dialogService,
        object viewModel,
        string title,
        Func<ICloseable, IOption[]> optionsFactory)
    {
        if (dialogService == null) throw new ArgumentNullException(nameof(dialogService));
        if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
        if (optionsFactory == null) throw new ArgumentNullException(nameof(optionsFactory));

        return dialogService.Show(viewModel, Observable.Return(title), closeable => optionsFactory(closeable));
    }

    // New overload: reactive titles
    public static Task<bool> Show(this IDialog dialogService,
        object viewModel,
        IObservable<string> title,
        Func<ICloseable, IEnumerable<IOption>> optionsFactory)
    {
        if (dialogService == null) throw new ArgumentNullException(nameof(dialogService));
        if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
        if (title == null) throw new ArgumentNullException(nameof(title));
        if (optionsFactory == null) throw new ArgumentNullException(nameof(optionsFactory));

        return dialogService.Show(viewModel, title, optionsFactory);
    }

    // Convenience: ViewModels that expose a title
    public static Task<bool> Show(this IDialog dialogService,
        IHaveTitle viewModel,
        Func<ICloseable, IOption[]> optionsFactory)
    {
        if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

        return dialogService.Show(viewModel, viewModel.Title, closeable => optionsFactory(closeable));
    }

    public static Task ShowOk(this IDialog dialogService,
        object viewModel,
        string title,
        IObservable<bool>? canSubmit = null)
    {
        return dialogService.Show(viewModel, title, closeable =>
        {
            IEnhancedCommand command = ReactiveCommand.Create(closeable.Close, canSubmit).Enhance();
            Settings settings = new Settings()
            {
                IsDefault = true,
            };
            return
            [
                new Option("OK", command, settings)
            ];
        });
    }

    public static Task Show(this IDialog dialogService,
        object viewModel,
        string title,
        IObservable<bool>? canSubmit)
    {
        return dialogService.Show(viewModel, title, closeable =>
        [
            Cancel(closeable),
            Ok(closeable, canSubmit)
        ]);
    }

    private static Option Cancel(ICloseable closeable)
    {
        Settings settings = new Settings
        {
            IsDefault = false,
            IsCancel = true,
            Role = OptionRole.Cancel,
        };
        IEnhancedCommand command = ReactiveCommand.Create(closeable.Dismiss, Observable.Return(true)).Enhance();
        return new Option("Cancel", command, settings);
    }

    private static Option Ok(ICloseable closeable, IObservable<bool>? canClose)
    {
        IEnhancedCommand command = ReactiveCommand.Create(closeable.Close, canClose).Enhance();
        return new Option("OK", command, new Settings() { IsDefault = true });
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, TResult> getResult)
    {
        return await dialogService.ShowAndGetResult(viewModel, Observable.Return(title), optionsFactory, getResult);
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, Task<TResult>> getResult)
    {
        return await dialogService.ShowAndGetResult(viewModel, Observable.Return(title), optionsFactory, getResult);
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, IObservable<bool>> canSubmit,
        Func<TViewModel, TResult> getResult)
    {
        Func<ICloseable, IOption[]> optionsFactory = closeable =>
        [
            Cancel(closeable),
            Ok(closeable, canSubmit(viewModel))
        ];

        return await dialogService.ShowAndGetResult(viewModel, Observable.Return(title), optionsFactory, getResult);
    }

    /// <summary>
    /// Shows a dialog with the given IValidatable viewModel and returns the result when the user confirms.
    /// Uses the viewModel's IsValid property for submit validation.
    /// </summary>
    public static Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, TResult> getResult) where TViewModel : IValidatable
    {
        return dialogService.ShowAndGetResult(viewModel, title, vm => vm.IsValid, getResult);
    }

    /// <summary>
    /// Shows a dialog with the given IValidatable viewModel and returns the async result when the user confirms.
    /// Uses the viewModel's IsValid property for submit validation.
    /// </summary>
    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, Task<TResult>> getResult) where TViewModel : IValidatable
    {
        Func<ICloseable, IOption[]> optionsFactory = closeable =>
        [
            Cancel(closeable),
            Ok(closeable, viewModel.IsValid)
        ];

        return await dialogService.ShowAndGetResult(viewModel, Observable.Return(title), optionsFactory, getResult);
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        IObservable<string> title,
        Func<ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, TResult> getResult)
    {
        bool showed = await dialogService.Show(viewModel, title, optionsFactory);

        if (showed)
        {
            return getResult(viewModel);
        }

        return Maybe<TResult>.None;
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        IObservable<string> title,
        Func<ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, Task<TResult>> getResult)
    {
        bool showed = await dialogService.Show(viewModel, title, optionsFactory);

        if (showed)
        {
            return await getResult(viewModel);
        }

        return Maybe<TResult>.None;
    }

    public static async Task<Maybe<bool>> ShowConfirmation(this IDialog dialogService, string title, string text, string yesText = "Yes", string noText = "No")
    {
        var result = false;

        var show = await dialogService.Show(new MessageDialogViewModel(text), title, closeable =>
        {
            List<IOption> options =
            [
                new Option(yesText, ReactiveCommand.Create(() =>
                {
                    result = true;
                    closeable.Close();
                }).Enhance(), new Settings()),
                new Option(noText, ReactiveCommand.Create(() =>
                {
                    result = false;
                    closeable.Close();
                }).Enhance(), new Settings())
            ];

            return options.ToArray();
        });

        if (show)
        {
            return result;
        }

        return Maybe<bool>.None;
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, IEnhancedCommand<Result<TResult>>> getResultCommand)
    {
        return await dialogService.ShowAndGetResult(viewModel, Observable.Return(title), getResultCommand);
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        IObservable<string> title,
        Func<TViewModel, IEnhancedCommand<Result<TResult>>> getResultCommand)
    {
        var command = getResultCommand(viewModel);
        var captured = Maybe<TResult>.None;
        IDisposable? subscription = null;

        var success = await dialogService.Show(viewModel, title, closeable =>
        {
            subscription = command
                .Successes()
                .Take(1)
                .Subscribe(value =>
                {
                    captured = value;
                    closeable.Close();
                });

            return
            [
                Cancel(closeable),
                new Option(command.Text ?? "OK", command, new Settings { IsDefault = true }),
            ];
        });

        subscription?.Dispose();
        return success ? captured : Maybe<TResult>.None;
    }

    public static Task ShowMessage(this IDialog dialogService,
        string title,
        string text,
        string okText = "OK")
    {
        var messageDialogViewModel = new MessageDialogViewModel(text);

        return dialogService.Show(messageDialogViewModel, title, closeable =>
        {
            IEnhancedCommand command = ReactiveCommand.Create(closeable.Close, Observable.Return(true)).Enhance();
            Settings settings = new Settings()
            {
                IsDefault = true,
                IsCancel = true,
            };
            return
            [
                new Option(okText, command, settings)
            ];
        });
    }
}