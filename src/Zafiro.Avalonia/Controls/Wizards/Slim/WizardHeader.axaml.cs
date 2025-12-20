using Avalonia.Controls.Primitives;
using Zafiro.UI.Wizards.Slim;

namespace Zafiro.Avalonia.Controls.Wizards.Slim;

public class WizardHeader : TemplatedControl
{
    public static readonly StyledProperty<bool> IsBackButtonVisibleProperty =
        AvaloniaProperty.Register<WizardHeader, bool>(nameof(IsBackButtonVisible), defaultValue: true);

    public static readonly StyledProperty<ISlimWizard> WizardProperty = AvaloniaProperty.Register<WizardHeader, ISlimWizard>(
        nameof(Wizard));

    public bool IsBackButtonVisible
    {
        get => GetValue(IsBackButtonVisibleProperty);
        set => SetValue(IsBackButtonVisibleProperty, value);
    }

    public ISlimWizard Wizard
    {
        get => GetValue(WizardProperty);
        set => SetValue(WizardProperty, value);
    }
}