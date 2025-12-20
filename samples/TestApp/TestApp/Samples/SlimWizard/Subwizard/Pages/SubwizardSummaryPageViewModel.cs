using System;
using System.Reactive.Linq;
using Zafiro.UI;

namespace TestApp.Samples.SlimWizard.Subwizard.Pages;

public class SubwizardSummaryPageViewModel(string childResult) : IHaveTitle
{
    public string ChildResult { get; } = childResult;

    public IObservable<string> Title { get; } = Observable.Return("Summary");
}