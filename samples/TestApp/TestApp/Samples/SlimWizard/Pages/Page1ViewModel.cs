using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Zafiro.UI;
using Zafiro.UI.Commands;
using Zafiro.UI.Navigation;

namespace TestApp.Samples.SlimWizard.Pages;

public partial class Page1ViewModel : ReactiveValidationObject, IHaveTitle, IHaveHeader, IHaveFooter
{
    [Reactive] private int? number;

    public Page1ViewModel()
    {
        this.ValidationRule(x => x.Number, i => i % 2 == 0, "Number must be even");
        ReturnSomeInt = ReactiveCommand.CreateFromTask(async () =>
        {
            await Task.Delay(1000);
            return Result.Success(Number!.Value);
        }, this.IsValid()).Enhance();
    }

    public IEnhancedCommand<Result<int>> ReturnSomeInt { get; set; }

    public IObservable<bool> IsValid => this.IsValid();
    public IObservable<bool> IsBusy => Observable.Return(false);
    public bool AutoAdvance => false;
    public object Footer => "This is the footer";

    public object Header => "This is the header";

    // Reactive title example: reflects the current number as the user types
    public IObservable<string> Title => this
        .WhenAnyValue(x => x.Number)
        .Select(n => n.HasValue
            ? $"First page (current value: {n.Value})"
            : "First page (enter an even number)");
}