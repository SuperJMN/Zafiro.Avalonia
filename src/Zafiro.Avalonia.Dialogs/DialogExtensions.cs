using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using CSharpFunctionalExtensions;
using ReactiveUI;
using Zafiro.Avalonia.Dialogs.Views;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Dialogs;

public static class DialogExtensions
{
    public static Task<bool> Show(this IDialog dialogService,
        object viewModel,
        string title,
        Func<ICloseable, IOption[]> optionsFactory)
    {
        return dialogService.Show(viewModel, title, optionsFactory);
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
        bool showed = await dialogService.Show(viewModel, title, optionsFactory);

        if (showed)
        {
            return getResult(viewModel);
        }

        return Maybe<TResult>.None;
    }

    public static async Task<Maybe<TResult>> ShowAndGetResult<TViewModel, TResult>(this IDialog dialogService,
        [DisallowNull] TViewModel viewModel,
        string title,
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

        return await dialogService.ShowAndGetResult(viewModel, title, optionsFactory, getResult);
    }

    public static async Task<Maybe<bool>> ShowConfirmation(this IDialog dialogService, string title, string text, string yesText = "Yes", string noText = "No")
    {
        var result = false;

        var content = new MessageDialogView
        {
            Text = text
        };

        var show = await dialogService.Show(content, title, closeable =>
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

            return options;
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
        var messageDialogView = new MessageDialogView
        {
            Text = text
        };

        return dialogService.Show(messageDialogView, title, closeable =>
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