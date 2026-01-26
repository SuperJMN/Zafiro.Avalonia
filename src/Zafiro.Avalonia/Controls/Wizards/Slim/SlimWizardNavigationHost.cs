using Zafiro.UI.Commands;
using Zafiro.UI.Navigation;
using Zafiro.UI.Wizards.Slim;

namespace Zafiro.Avalonia.Controls.Wizards.Slim;

public sealed class SlimWizardNavigationHost : IBackCommandProvider, IHaveHeader, IHaveFooter
{
    public SlimWizardNavigationHost(ISlimWizard wizard, IEnhancedCommand cancel)
    {
        Wizard = wizard ?? throw new ArgumentNullException(nameof(wizard));
        Cancel = cancel ?? throw new ArgumentNullException(nameof(cancel));

        var isFirstPage = wizard
            .WhenAnyValue(w => w.CurrentPage.Index)
            .Select(index => index == 0);

        var canGoBackInWizard = ((IReactiveCommand)wizard.Back).CanExecute;
        var canExecute = canGoBackInWizard.CombineLatest(isFirstPage, (canGoBack, firstPage) => canGoBack || firstPage);

        Back = ReactiveCommand.Create(ExecuteBack, canExecute).Enhance();
    }

    public ISlimWizard Wizard { get; }

    public IEnhancedCommand Cancel { get; }

    public IEnhancedCommand Back { get; }

    public IObservable<object> Footer => Observable.Return(new WizardFooterViewModel(this));

    public IObservable<object> Header => Observable.Return(new WizardHeaderViewModel(Wizard));

    private void ExecuteBack()
    {
        if (Wizard.Back.CanExecute(null))
        {
            Wizard.Back.Execute(null);
            return;
        }

        if (Wizard.CurrentPage.Index == 0)
        {
            Cancel.Execute(null);
        }
    }
}

public record WizardHeaderViewModel(ISlimWizard Wizard);

public record WizardFooterViewModel(SlimWizardNavigationHost Host);