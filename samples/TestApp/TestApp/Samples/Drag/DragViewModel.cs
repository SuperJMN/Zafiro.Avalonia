using System;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Zafiro.Avalonia;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Drag;

[Section(icon: "mdi-cursor-pointer", sortIndex: 18)]
public partial class DragViewModel : ReactiveObject
{
    [Reactive] private bool isEnabled;
    [Reactive] private double left;

    public DragViewModel()
    {
        Observable.Interval(TimeSpan.FromMilliseconds(24), AvaloniaScheduler.Instance).Subscribe(_ =>
        {
            if (IsEnabled)
            {
                Left += 1;
            }
        });

    }

    public ICommand Enable => ReactiveCommand.Create(() => IsEnabled = true);
    public ICommand Disable => ReactiveCommand.Create(() => IsEnabled = false);
}