using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CSharpFunctionalExtensions;
using ReactiveUI;
using Zafiro.UI;
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
        // - Si el contenido actual implementa IHaveTitle, se usa su Title (IObservable<string>).
        // - En otro caso, se usa el título explícito pasado al método.
        var dialogTitle = wizard
            .WhenAnyValue(slimWizard => slimWizard.CurrentPage.Content)
            .Select(content =>
                content is IHaveTitle haveTitle
                    ? haveTitle.Title
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