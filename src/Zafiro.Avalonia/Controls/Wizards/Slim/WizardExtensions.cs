using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Misc;
using Zafiro.UI.Commands;
using Zafiro.UI.Navigation;
using Zafiro.UI.Wizards.Slim;

namespace Zafiro.Avalonia.Controls.Wizards.Slim;

public static class WizardExtensions
{
    public static async Task<Maybe<T>> Navigate<T>(this ISlimWizard<T> wizard, INavigator navigator, Func<ISlimWizard<T>, INavigator, Task<bool>>? cancel = null)
    {
        var cancelHandler = cancel ?? DefaultCancel<T>;

        using var session = new WizardNavigationSession<T>(
            wizard,
            navigator,
            cancelCommand => ApplicationUtils.ExecuteOnUIThread(() => CreateUserControl(wizard, cancelCommand)),
            cancelHandler);

        var startResult = await session.StartAsync();
        if (startResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to navigate to wizard: {startResult.Error}");
        }

        return await session.Completion;
    }

    private static UserControl CreateUserControl<T>(ISlimWizard<T> wizard, IEnhancedCommand cancelCommand)
    {
        return new UserControl
        {
            Content = new WizardNavigator
            {
                Wizard = wizard,
                Cancel = cancelCommand
            }
        };
    }

    private static Task<bool> DefaultCancel<T>(ISlimWizard<T> _, INavigator navigator) => Task.FromResult(true);
}