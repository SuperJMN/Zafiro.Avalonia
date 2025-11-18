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

        // Título del diálogo:
        // - Se usa el TitleObservable de la página actual construido por el Slim wizard builder.
        // - Si por cualquier motivo la página no implementa IPage (no debería pasar en SlimWizard),
        //   se usa el título global pasado al método como fallback.
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