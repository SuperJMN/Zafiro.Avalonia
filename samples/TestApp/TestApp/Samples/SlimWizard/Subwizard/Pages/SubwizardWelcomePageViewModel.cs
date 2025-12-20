using System;
using System.Reactive.Linq;
using Zafiro.UI;

namespace TestApp.Samples.SlimWizard.Subwizard.Pages;

public class SubwizardWelcomePageViewModel : IHaveTitle
{
    public IObservable<string> Title { get; } = Observable.Return("Welcome");
}