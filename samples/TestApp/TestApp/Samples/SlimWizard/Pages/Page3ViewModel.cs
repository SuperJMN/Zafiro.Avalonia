using System;
using System.Reactive.Linq;
using ReactiveUI.Validation.Helpers;
using Zafiro.UI;

namespace TestApp.Samples.SlimWizard.Pages;

public class Page3ViewModel : ReactiveValidationObject, IValidatable, IHaveTitle
{
    public IObservable<bool> IsBusy => Observable.Return(false);
    public bool AutoAdvance => false;
    public IObservable<string> Title { get; } = Observable.Return("Final page");
    public IObservable<bool> IsValid { get; } = Observable.Return(true);
}