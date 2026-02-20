using System.Reactive.Linq;

namespace Zafiro.Avalonia.Dialogs;

public class Settings
{
    public bool IsDefault { get; init; }
    public bool IsCancel { get; init; }
    public IObservable<bool> IsVisible { get; init; } = Observable.Return(true);

    public OptionRole Role { get; init; }
    public object? Icon { get; init; }
}