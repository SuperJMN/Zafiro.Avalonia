using System.Reactive.Linq;
using ReactiveUI;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Dialogs;

public class OptionDesign : IOption
{
    public string Title { get; set; } = "";
    IObservable<string> IOption.Title => Observable.Return(Title);
    public IEnhancedCommand Command { get; } = ReactiveCommand.Create(() => { }).Enhance();
    public bool IsDefault { get; set; }
    public bool IsCancel { get; set; }
    public IObservable<bool> IsVisible { get; set; }
    public OptionRole Role { get; set; }
}