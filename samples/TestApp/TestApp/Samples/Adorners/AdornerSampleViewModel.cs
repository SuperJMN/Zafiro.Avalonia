using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Adorners;

[Section(icon: "mdi-bee", sortIndex: 21)]
[SectionGroup("behaviors", "Behaviors")]
public class AdornerSampleViewModel : ReactiveObject
{
    public AdornerSampleViewModel()
    {
        LengthyCommand = ReactiveCommand.CreateFromTask(() => Task.Delay(1000));
        IsExecuting = LengthyCommand.IsExecuting;
    }

    public IObservable<bool> IsExecuting { get; set; }

    public ReactiveCommand<Unit, Unit> LengthyCommand { get; set; }
}