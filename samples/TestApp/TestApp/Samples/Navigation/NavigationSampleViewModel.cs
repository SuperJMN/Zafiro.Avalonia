using System;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using Zafiro.UI.Navigation;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Navigation;

[Section(icon: "mdi-chevron-right", sortIndex: 14)]
[SectionGroup("navigation", "Navigation & Dialogs")]
public class NavigationSampleViewModel(INavigator navigator) : ReactiveObject, IHaveHeader, IHaveFooter
{
    public ICommand Navigate => ReactiveCommand.CreateFromTask(() => navigator.Go<TargetViewModel>());
    public IObservable<object> Footer => Observable.Return("This is a Footer");
    public IObservable<object> Header => Observable.Return("This is a Header");
}