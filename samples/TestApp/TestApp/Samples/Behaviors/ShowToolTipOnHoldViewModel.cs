using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Behaviors;

[Section("Hold Tooltip", icon: "mdi-cursor-pointer", sortIndex: 19)]
[SectionGroup("behaviors", "Behaviors")]
public partial class ShowToolTipOnHoldViewModel : ReactiveObject
{
    [Reactive] private int activations;

    public ShowToolTipOnHoldViewModel()
    {
        Activate = ReactiveCommand.Create(() => Activations++);
    }

    public ICommand Activate { get; }
}
