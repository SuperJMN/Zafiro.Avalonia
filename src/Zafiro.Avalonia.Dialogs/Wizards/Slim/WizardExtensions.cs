using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CSharpFunctionalExtensions;
using ReactiveUI;
using Zafiro.UI.Commands;
using Zafiro.UI.Wizards.Slim;

namespace Zafiro.Avalonia.Dialogs.Wizards.Slim;

public static class WizardExtensions
{
    public static async Task<Maybe<TResult>> ShowWizard<TResult>(this IDialog dialog, ISlimWizard<TResult> wizard, string title)
    {
        var disposables = new CompositeDisposable();

        var nextOption = new NextOption(wizard).DisposeWith(disposables);

        Func<ICloseable, IEnumerable<IOption>> optionsFactory = closeable =>
        {
            var canCancel = wizard.WhenAnyValue(slimWizard => slimWizard.CurrentPage).Select(x => x.Index != wizard.TotalPages - 1);
            var cancel = ReactiveCommand.Create(closeable.Dismiss, canCancel).Enhance();
            wizard.Finished.Subscribe(_ => closeable.Close()).DisposeWith(disposables);

            Settings settings = new Settings
            {
                IsCancel = true,
                Role = OptionRole.Cancel,
                IsVisible = canCancel,
            };
            return
            [
                nextOption,
                new Option("Cancel", cancel, settings),
            ];
        };

        // Dialog title:
        // - Use the TitleObservable of the current page produced by the Slim wizard builder.
        // - If for any reason the page does not implement IPage (which should not happen in SlimWizard),
        //   fall back to the global title passed to the method.
        var dialogTitle = wizard
            .WhenAnyValue(slimWizard => slimWizard.CurrentPage)
            .Select(page => page is IPage p
                ? p.TitleObservable
                : Observable.Return(title))
            .Switch()
            .DistinctUntilChanged();

        var showAndGetResult = await dialog.ShowAndGetResult(wizard, dialogTitle, optionsFactory, x => x.Finished.FirstAsync().ToTask());

        disposables.Dispose();

        return showAndGetResult;
    }

    public static Task<Maybe<T>> ShowInDialog<T>(this ISlimWizard<T> wizard, IDialog navigator, string title)
    {
        return navigator.ShowWizard(wizard, title);
    }
}