using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Zafiro.UI;
using Zafiro.UI.Navigation;

namespace TestApp.Samples.SlimWizard.Pages;

public partial class Page2ViewModel : ReactiveValidationObject, IValidatable, IHaveTitle, IHaveHeader, IHaveFooter
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
    public IObservable<object> Footer => Observable.Return("This is the footer of page 2");

    public IObservable<object> Header => Observable.Return("This is the header of page 2");

    // Reactive title example: changes depending on whether the checkbox is checked
    public IObservable<string> Title => this
        .WhenAnyValue(x => x.IsChecked)
        .Select(isCheckedValue => isCheckedValue
            ? $"Second page (ready to continue with {Number})"
            : $"Second page (please check the box for {Number})");

    public IObservable<bool> IsValid => this.IsValid();
}