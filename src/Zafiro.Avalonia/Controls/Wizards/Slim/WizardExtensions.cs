using CSharpFunctionalExtensions;
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
            cancelCommand => new NavigationWizardHost(wizard, cancelCommand),
            cancelHandler);

        var startResult = await session.StartAsync();
        if (startResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to navigate to wizard: {startResult.Error}");
        }

        return await session.Completion;
    }

    private static Task<bool> DefaultCancel<T>(ISlimWizard<T> _, INavigator navigator) => Task.FromResult(true);
}