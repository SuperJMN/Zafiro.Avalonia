using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Zafiro.UI;

namespace TestApp.Samples.SlimWizard.Subwizard.Pages;

public partial class ChildWizardInputPageViewModel : ReactiveObject, IHaveTitle, IValidatable
{
    private readonly string kind;

    [Reactive] private string? result;

    public ChildWizardInputPageViewModel(string kind)
    {
        this.kind = kind;

        Title = Observable.Return($"Child wizard {kind}");
        IsValid = this.WhenAnyValue(x => x.Result).Select(text => !string.IsNullOrWhiteSpace(text));
    }

    public string Prompt => $"Enter a value for child wizard {kind}";

    public IObservable<string> Title { get; }

    public IObservable<bool> IsValid { get; }
}