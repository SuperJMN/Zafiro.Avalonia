using CSharpFunctionalExtensions;

namespace Zafiro.Avalonia.Dialogs;

public interface IDialog
{
    Task<bool> Show<TViewModel>(Maybe<TViewModel> viewModel, Maybe<IObservable<string>> title, Func<Maybe<TViewModel>, ICloseable, IEnumerable<IOption>> optionsFactory, Maybe<object> icon = default, DialogTone tone = DialogTone.Neutral);
}