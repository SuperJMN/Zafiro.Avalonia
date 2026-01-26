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

        // Wrapped wizard to provide footer content in a dialog-friendly way
        var dialogHost = new DialogWizardHost(wizard);

        Func<ICloseable, IEnumerable<IOption>> optionsFactory = closeable =>
        {
            var canCancel = wizard.WhenAnyValue(slimWizard => slimWizard.CurrentPage).Select(x => x.Index != wizard.TotalPages - 1);
            var cancel = ReactiveCommand.Create(closeable.Dismiss, canCancel).Enhance();
            wizard.Finished.Subscribe(_ => closeable.Close()).DisposeWith(disposables);

            return
            [
                new Option("Cancel", cancel, new Settings { IsCancel = true, Role = OptionRole.Cancel, IsVisible = canCancel }),
                new Option("Back", wizard.Back, new Settings { Role = OptionRole.Primary }),
                new Option("Next", wizard.Next, new Settings { Role = OptionRole.Primary, IsDefault = true }),
            ];
        };

        // Dialog title:
        // - Uses the TitleObservable of the current page built by the Slim wizard builder.
        // - If for any reason the page does not implement IPage (it should not happen in SlimWizard),
        //   the global title passed to the method is used as a fallback.
        var dialogTitle = wizard
            .WhenAnyValue(slimWizard => slimWizard.CurrentPage)
            .Select(page => page is IPage p
                ? p.TitleObservable
                : Observable.Return(title))
            .Switch()
            .DistinctUntilChanged();

        var showAndGetResult = await dialog.ShowAndGetResult(dialogHost, dialogTitle, optionsFactory, x => wizard.Finished.FirstAsync().ToTask());

        disposables.Dispose();

        return showAndGetResult;
    }

    public static Task<Maybe<T>> ShowInDialog<T>(this ISlimWizard<T> wizard, IDialog navigator, string title)
    {
        return navigator.ShowWizard(wizard, title);
    }
}