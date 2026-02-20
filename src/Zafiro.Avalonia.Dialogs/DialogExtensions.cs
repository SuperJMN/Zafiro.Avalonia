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
    #region Show Overloads

    public static Task<bool> Show<TViewModel>(this IDialog dialog, TViewModel? viewModel, string title, Func<TViewModel?, ICloseable, IEnumerable<IOption>> optionsFactory, object? icon = null, DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return dialog.Show(viewModel, Observable.Return(title), optionsFactory, icon, tone);
    }

    public static Task<bool> Show<TViewModel>(this IDialog dialog, TViewModel viewModel, Func<TViewModel?, ICloseable, IEnumerable<IOption>> optionsFactory, object? icon = null, DialogTone tone = DialogTone.Neutral) where TViewModel : class, IHaveTitle
    {
        return dialog.Show(viewModel, viewModel.Title, optionsFactory, icon, tone);
    }

    public static Task<bool> Show<TViewModel>(this IDialog dialog, TViewModel? viewModel, string title, Func<ICloseable, IEnumerable<IOption>> optionsFactory, object? icon = null, DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return dialog.Show(viewModel, Observable.Return(title), (_, closeable) => optionsFactory(closeable), icon, tone);
    }

    public static Task<bool> Show<TViewModel>(this IDialog dialog, TViewModel viewModel, Func<ICloseable, IEnumerable<IOption>> optionsFactory, object? icon = null, DialogTone tone = DialogTone.Neutral) where TViewModel : class, IHaveTitle
    {
        return dialog.Show(viewModel, viewModel.Title, (_, closeable) => optionsFactory(closeable), icon, tone);
    }

    public static Task<bool> Show(this IDialog dialog, string title, Func<ICloseable, IEnumerable<IOption>> optionsFactory, object? icon = null, DialogTone tone = DialogTone.Neutral)
    {
        return dialog.Show((object?)null, Observable.Return(title), (_, closeable) => optionsFactory(closeable), icon, tone);
    }

    public static Task<bool> Show(this IDialog dialog, IObservable<string> title, Func<ICloseable, IEnumerable<IOption>> optionsFactory, object? icon = null, DialogTone tone = DialogTone.Neutral)
    {
        return dialog.Show((object?)null, title, (_, closeable) => optionsFactory(closeable), icon, tone);
    }

    #endregion

    #region ShowOk / ShowOkCancel

    public static Task ShowOk<TViewModel>(this IDialog dialogService, TViewModel? viewModel, string title, Func<TViewModel?, IObservable<bool>>? canSubmit = null, object? icon = null, DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return dialogService.ShowOk(viewModel, Observable.Return(title), canSubmit, icon, tone);
    }

    public static Task ShowOk<TViewModel>(this IDialog dialogService, TViewModel viewModel, Func<TViewModel?, IObservable<bool>>? canSubmit = null, object? icon = null, DialogTone tone = DialogTone.Neutral) where TViewModel : class, IHaveTitle
    {
        return dialogService.ShowOk(viewModel, viewModel.Title, canSubmit, icon, tone);
    }

    public static Task ShowOk<TViewModel>(this IDialog dialogService, TViewModel? viewModel, IObservable<string> title, Func<TViewModel?, IObservable<bool>>? canSubmit = null, object? icon = null, DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return dialogService.Show(viewModel, title, (vm, closeable) =>
        {
            var canExecute = canSubmit?.Invoke(vm) ?? Observable.Return(true);
            return [Ok(closeable, canExecute)];
        }, icon, tone);
    }

    public static Task ShowOk(this IDialog dialogService, string title, IObservable<bool>? canSubmit = null, object? icon = null, DialogTone tone = DialogTone.Neutral)
    {
        return dialogService.ShowOk((object?)null, Observable.Return(title), _ => canSubmit ?? Observable.Return(true), icon, tone);
    }

    public static Task Show<TViewModel>(this IDialog dialogService,
        TViewModel? viewModel,
        string title,
        IObservable<bool>? canSubmit,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return dialogService.Show(viewModel, Observable.Return(title), canSubmit, icon, tone);
    }

    public static Task Show<TViewModel>(this IDialog dialogService,
        TViewModel? viewModel,
        IObservable<string> title,
        IObservable<bool>? canSubmit,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return dialogService.Show(viewModel, title, (_, closeable) =>
        [
            Cancel(closeable),
            Ok(closeable, canSubmit ?? Observable.Return(true))
        ], icon, tone);
    }

    #endregion

    #region ShowAndGetResult

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        IObservable<string> title,
        Func<TViewModel?, ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, Task<TResult>> getResult,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        var isSuccess = await dialogService.Show(viewModel, title, optionsFactory, icon, tone);
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
        Func<TViewModel, Task<TResult>> getResult,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return dialogService.ShowAndGetResult(viewModel, title, (_, closeable) => optionsFactory(closeable), getResult, icon, tone);
    }

    public static Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel?, ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, Task<TResult>> getResult,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return dialogService.ShowAndGetResult(viewModel, Observable.Return(title), optionsFactory, getResult, icon, tone);
    }

    public static Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        IObservable<string> title,
        Func<TViewModel?, ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, TResult> getResult,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return dialogService.ShowAndGetResult(viewModel, title, optionsFactory, vm => Task.FromResult(getResult(vm)), icon, tone);
    }

    public static Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel?, ICloseable, IEnumerable<IOption>> optionsFactory,
        Func<TViewModel, TResult> getResult,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return dialogService.ShowAndGetResult(viewModel, Observable.Return(title), optionsFactory, vm => Task.FromResult(getResult(vm)), icon, tone);
    }

    // Convenience for IValidatable
    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, Task<TResult>> getResult,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class, IValidatable
    {
        return await dialogService.ShowAndGetResult(viewModel, Observable.Return(title), (vm, closeable) =>
        [
            Cancel(closeable),
            Ok(closeable, vm!.IsValid)
        ], getResult, icon, tone);
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, TResult> getResult,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class, IValidatable
    {
        return await dialogService.ShowAndGetResult(viewModel, Observable.Return(title), (vm, closeable) =>
        [
            Cancel(closeable),
            Ok(closeable, vm!.IsValid)
        ], vm => Task.FromResult(getResult(vm)), icon, tone);
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel?, IObservable<bool>> isValid,
        Func<TViewModel, TResult> getResult,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return await dialogService.ShowAndGetResult(viewModel, Observable.Return(title), (vm, closeable) =>
        [
            Cancel(closeable),
            Ok(closeable, isValid(vm))
        ], vm => Task.FromResult(getResult(vm)), icon, tone);
    }

    // Command Result Variant
    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        IObservable<string> title,
        Func<TViewModel, IEnhancedCommand<Result<TResult>>> getResultCommand,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        var command = getResultCommand(viewModel);
        var captured = Maybe<TResult>.None;
        IDisposable? subscription = null;

        var success = await dialogService.Show(viewModel, title, (_, closeable) =>
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
        }, icon, tone);

        subscription?.Dispose();
        return success ? captured : Maybe<TResult>.None;
    }

    public static Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
        Func<TViewModel, IEnhancedCommand<Result<TResult>>> getResultCommand,
        object? icon = null,
        DialogTone tone = DialogTone.Neutral) where TViewModel : class
    {
        return dialogService.ShowAndGetResult(viewModel, Observable.Return(title), getResultCommand, icon, tone);
    }

    #endregion

    #region Specialized Dialogs

    public static Task ShowMessage(this IDialog dialogService, string title, string text, string okText = "OK", object? icon = null, DialogTone tone = DialogTone.Information)
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
        }, icon, tone);
    }

    public static async Task<Maybe<bool>> ShowConfirmation(this IDialog dialogService, string title, string text, string yesText = "Yes", string noText = "No", bool yesIsPrimary = true, object? icon = null, DialogTone tone = DialogTone.Neutral)
    {
        var result = false;
        var yesRole = yesIsPrimary ? OptionRole.Primary : OptionRole.Secondary;
        var noRole = yesIsPrimary ? OptionRole.Secondary : OptionRole.Primary;

        var show = await dialogService.Show(new MessageDialogViewModel(text), title, (_, closeable) =>
        {
            return
            [
                new Option(yesText, ReactiveCommand.Create(() =>
                {
                    result = true;
                    closeable.Close();
                }).Enhance(), new Settings { Role = yesRole }),

                new Option(noText, ReactiveCommand.Create(() =>
                {
                    result = false;
                    closeable.Close();
                }).Enhance(), new Settings { Role = noRole })
            ];
        }, icon, tone);

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