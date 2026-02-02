using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Dialogs;

public partial class OptionDesign : ReactiveObject, IOption
{
    [Reactive] private string title = "<Button>";

    IObservable<string> IOption.Title => this.WhenAnyValue(x => x.Title);
    public IEnhancedCommand Command { get; } = ReactiveCommand.Create(() => { }).Enhance();
    public bool IsDefault { get; set; }
    public bool IsCancel { get; set; }
    public IObservable<bool> IsVisible { get; } = Observable.Return(true);
    public OptionRole Role { get; set; }
    public object? Icon { get; set; }
}