using System;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Zafiro.UI.Navigation;

// Ensure this namespace is correct for EnhancedCommand

namespace TestApp.Samples.GraphWizard;

public class Step1ViewModel : ReactiveObject, IHaveFooter, IHaveHeader
{
    private string? choice;

    public Step1ViewModel()
    {
        ChooseA = ReactiveCommand.Create(() => Choice = "A");
        ChooseB = ReactiveCommand.Create(() => Choice = "B");
        // We'll expose a 'Next' command or just let the wizard observe 'Choice'?
        // The WizardNode expects to execute something to get the next node.
        // Usually, the page has a 'Next' button (or the wizard footer does).
        // If the wizard footer does, it executes WizardNode.Next.
        // WizardNode.Next calls the factory.

        // Wait, the Wizard Footer (in GraphWizardView) binds to Wizard.Next.
        // Wizard.Next executes CurrentStep.Next.
        // CurrentStep.Next (in WizardNode) executes the factory.

        // So the factory needs access to the VM state to decide.
        // That is fine.

        // But what triggers the "Validation" or "Completion" of the step?
        // The WizardNode has 'CanNext' observable.
        // So we should expose IsValid.
    }

    public string? Choice
    {
        get => choice;
        private set => this.RaiseAndSetIfChanged(ref choice, value);
    }

    public ReactiveCommand<Unit, string> ChooseA { get; }
    public ReactiveCommand<Unit, string> ChooseB { get; }
    public IObservable<object> Footer => Observable.Return("This is the footer");
    public IObservable<object> Header => Observable.Return("This is the header");
}