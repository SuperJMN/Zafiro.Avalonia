using System;
using System.Reactive.Linq;
using Zafiro.UI.Navigation;

namespace TestApp.Samples.Navigation;

public class TargetViewModel : IHaveHeader, IHaveFooter
{
    public IObservable<object> Footer => Observable.Return("Footer (Target)");
    public IObservable<object> Header => Observable.Return("Header (Target)");
}