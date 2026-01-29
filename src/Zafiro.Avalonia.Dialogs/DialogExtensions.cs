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
    // Implementation of IDialog.Show<T> is used as the core for all these extensions.

    #region Show Overloads

    public static Task<bool> Show<TViewModel>(this IDialog dialog, TViewModel viewModel, string title, Func<TViewModel, ICloseable, IEnumerable<IOption>> optionsFactory) where TViewModel : class
    {
        return dialog.Show(viewModel, Observable.Return(title), optionsFactory);
    }

    public static Task<bool> Show<TViewModel>(this IDialog dialog, TViewModel viewModel, Func<TViewModel, ICloseable, IEnumerable<IOption>> optionsFactory) where TViewModel : class, IHaveTitle
    {
        return dialog.Show(viewModel, viewModel.Title, optionsFactory);
    }

    public static Task<bool> Show<TViewModel>(this IDialog dialog, TViewModel viewModel, string title, Func<ICloseable, IEnumerable<IOption>> optionsFactory) where TViewModel : class
    {
        return dialog.Show(viewModel, Observable.Return(title), (_, closeable) => optionsFactory(closeable));
    }

    public static Task<bool> Show<TViewModel>(this IDialog dialog, TViewModel viewModel, Func<ICloseable, IEnumerable<IOption>> optionsFactory) where TViewModel : class, IHaveTitle
    {
        return dialog.Show(viewModel, viewModel.Title, (_, closeable) => optionsFactory(closeable));
    }

    #endregion

    #region ShowOk / ShowOkCancel

    public static Task ShowOk<TViewModel>(this IDialog dialogService, TViewModel viewModel, string title, Func<TViewModel, IObservable<bool>>? canSubmit = null) where TViewModel : class
    {
        return dialogService.ShowOk(viewModel, Observable.Return(title), canSubmit);
    }

    public static Task ShowOk<TViewModel>(this IDialog dialogService, TViewModel viewModel, Func<TViewModel, IObservable<bool>>? canSubmit = null) where TViewModel : class, IHaveTitle
    {
        return dialogService.ShowOk(viewModel, viewModel.Title, canSubmit);
    }

    public static Task ShowOk<TViewModel>(this IDialog dialogService, TViewModel viewModel, IObservable<string> title, Func<TViewModel, IObservable<bool>>? canSubmit = null) where TViewModel : class
    {
        return dialogService.Show(viewModel, title, (vm, closeable) =>
        {
            var canExecute = canSubmit?.Invoke(vm) ?? Observable.Return(true);
            return [Ok(closeable, canExecute)];
        });
    }

    public static Task Show<TViewModel>(this IDialog dialogService,
        TViewModel viewModel,
        string title,
        IObservable<bool>? canSubmit) where TViewModel : class
    {
        return dialogService.Show(viewModel, Observable.Return(title), canSubmit);
    }

    public static Task Show<TViewModel>(this IDialog dialogService,
        TViewModel viewModel,
        IObservable<string> title,
        IObservable<bool>? canSubmit) where TViewModel : class
    {
        return dialogService.Show(viewModel, title, (_, closeable) =>
        [
            Cancel(closeable),
            Ok(closeable, canSubmit ?? Observable.Return(true))
        ]);
    }

    #endregion

    #region ShowAndGetResult

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        IObservable<string> title,
        Func<TViewModel, ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, Task<TResult>> getResult) where TViewModel : class
    {
        var isSuccess = await dialogService.Show(viewModel, title, optionsFactory);
        if (isSuccess)
        {
            return await getResult(viewModel);
        }

        return Maybe<TResult>.None;
    }

    public static Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        IObservable<string> title,
        Func<ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, Task<TResult>> getResult) where TViewModel : class
    {
        return dialogService.ShowAndGetResult(viewModel, title, (_, closeable) => optionsFactory(closeable), getResult);
    }

    public static Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, Task<TResult>> getResult) where TViewModel : class
    {
        return dialogService.ShowAndGetResult(viewModel, Observable.Return(title), optionsFactory, getResult);
    }

    public static Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        IObservable<string> title,
        Func<TViewModel, ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, TResult> getResult) where TViewModel : class
    {
        return dialogService.ShowAndGetResult(viewModel, title, optionsFactory, vm => Task.FromResult(getResult(vm)));
    }

    public static Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, TResult> getResult) where TViewModel : class
    {
        return dialogService.ShowAndGetResult(viewModel, Observable.Return(title), optionsFactory, vm => Task.FromResult(getResult(vm)));
    }

    // Convenience for IValidatable
    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, Task<TResult>> getResult) where TViewModel : class, IValidatable
    {
        return await dialogService.ShowAndGetResult(viewModel, Observable.Return(title), (vm, closeable) =>
        [
            Cancel(closeable),
            Ok(closeable, vm.IsValid)
        ], getResult);
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, TResult> getResult) where TViewModel : class, IValidatable
    {
        return await dialogService.ShowAndGetResult(viewModel, Observable.Return(title), (vm, closeable) =>
        [
            Cancel(closeable),
            Ok(closeable, vm.IsValid)
        ], vm => Task.FromResult(getResult(vm)));
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, IObservable<bool>> isValid,
        Func<TViewModel, TResult> getResult) where TViewModel : class
    {
        return await dialogService.ShowAndGetResult(viewModel, Observable.Return(title), (vm, closeable) =>
        [
            Cancel(closeable),
            Ok(closeable, isValid(vm))
        ], vm => Task.FromResult(getResult(vm)));
    }

    // Command Result Variant
    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        IObservable<string> title,
        Func<TViewModel, IEnhancedCommand<Result<TResult>>> getResultCommand) where TViewModel : class
    {
        var command = getResultCommand(viewModel);
        var captured = Maybe<TResult>.None;
        IDisposable? subscription = null;

        var success = await dialogService.Show(viewModel, title, (vm, closeable) =>
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

    public static Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, IEnhancedCommand<Result<TResult>>> getResultCommand) where TViewModel : class
    {
        return dialogService.ShowAndGetResult(viewModel, Observable.Return(title), getResultCommand);
    }

    #endregion

    #region Specialized Dialogs

    public static Task ShowMessage(this IDialog dialogService, string title, string text, string okText = "OK")
    {
        var messageDialogViewModel = new MessageDialogViewModel(text);

        return dialogService.Show(messageDialogViewModel, title, (_, closeable) =>
        {
            var command = ReactiveCommand.Create(closeable.Close).Enhance();
            var settings = new Settings
            {
                IsDefault = true,
                IsCancel = true
            };
            return [new Option(okText, command, settings)];
        });
    }

    public static async Task<Maybe<bool>> ShowConfirmation(this IDialog dialogService, string title, string text, string yesText = "Yes", string noText = "No")
    {
        var result = false;

        var show = await dialogService.Show(new MessageDialogViewModel(text), title, (_, closeable) =>
        {
            return
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
        });

        return show ? result : Maybe<bool>.None;
    }

    #endregion

    #region Private Helpers

    private static Option Cancel(ICloseable closeable)
    {
        var settings = new Settings
        {
            IsDefault = false,
            IsCancel = true,
            Role = OptionRole.Cancel,
        };
        var command = ReactiveCommand.Create(closeable.Dismiss).Enhance();
        return new Option("Cancel", command, settings);
    }

    private static Option Ok(ICloseable closeable, IObservable<bool> canClose)
    {
        var command = ReactiveCommand.Create(closeable.Close, canClose).Enhance();
        return new Option("OK", command, new Settings { IsDefault = true });
    }

    #endregion
}