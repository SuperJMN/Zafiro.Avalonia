using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Zafiro.UI;

namespace TestApp.Samples.SlimWizard.Pages;

public partial class Page2ViewModel : ReactiveValidationObject, IValidatable, IHaveTitle
{
    [Reactive] private bool isChecked;

    [Reactive] private string? text;

    public Page2ViewModel(int number)
    {
        Number = number;
        this.ValidationRule<Page2ViewModel, bool>(x => x.IsChecked, b => b, "Is must be checked");
    }

    public int Number { get; }

    public IObservable<bool> IsBusy => Observable.Return(false);
    public bool AutoAdvance => false;

    // Reactive title example: changes depending on whether the checkbox is checked
    public IObservable<string> Title => this
        .WhenAnyValue(x => x.IsChecked)
        .Select(isCheckedValue => isCheckedValue
            ? $"Second page (ready to continue with {Number})"
            : $"Second page (please check the box for {Number})");

    public IObservable<bool> IsValid => this.IsValid();
}