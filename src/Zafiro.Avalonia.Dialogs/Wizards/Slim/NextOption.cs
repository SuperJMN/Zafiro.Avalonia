using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Zafiro.UI.Commands;
using Zafiro.UI.Wizards.Slim;

namespace Zafiro.Avalonia.Dialogs.Wizards.Slim;

public partial class NextOption : ReactiveObject, IOption, IDisposable
{
    private readonly CompositeDisposable disposables = new();
    private readonly ISlimWizard wizard;
    [Reactive] private IEnhancedCommand command;

    public NextOption(ISlimWizard wizard)
    {
        this.wizard = wizard;
        this.WhenAnyValue(x => x.wizard.Next).BindTo<IEnhancedCommand, NextOption, IEnhancedCommand>(this, x => x.Command).DisposeWith(disposables);
    }

    public void Dispose()
    {
        disposables.Dispose();
    }

    public IObservable<string> Title => this.WhenAnyValue(x => x.wizard.Next.Text).Select(x => x ?? "");

    public bool IsDefault { get; } = true;
    public bool IsCancel { get; } = false;
    public IObservable<bool> IsVisible { get; } = Observable.Return(true);
    public OptionRole Role { get; } = OptionRole.Primary;
}