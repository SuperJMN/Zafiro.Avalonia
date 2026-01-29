namespace Zafiro.Avalonia.Dialogs;

public interface IDialog
{
    Task<bool> Show<TViewModel>(TViewModel viewModel, IObservable<string> title, Func<TViewModel, ICloseable, IEnumerable<IOption>> optionsFactory);
}