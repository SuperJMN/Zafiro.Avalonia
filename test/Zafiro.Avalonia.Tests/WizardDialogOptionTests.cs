using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Avalonia.Headless.XUnit;
using CSharpFunctionalExtensions;
using ReactiveUI;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.Dialogs.Wizards.Slim;
using Zafiro.UI.Commands;
using Zafiro.UI.Wizards.Slim;
using Zafiro.UI.Wizards.Slim.Builder;

namespace Zafiro.Avalonia.Tests;

public class WizardDialogOptionTests
{
    [AvaloniaFact]
    public async Task ShowWizard_uses_current_next_command_text_for_primary_option()
    {
        var dialog = new CapturingDialog();
        var install = ReactiveCommand.Create(() => Result.Success((object)"installed")).Enhance("Install");
        var wizard = new SlimWizard<string>(
            [
                new WizardStep(
                    StepKind.Completion,
                    "Ready to install",
                    _ => new object(),
                    _ => install,
                    _ => Observable.Return("Ready to install"))
            ],
            ImmediateScheduler.Instance);

        await dialog.ShowWizard(wizard, "Installer");

        var primary = Assert.Single(dialog.Options.Where(option => option.Role == OptionRole.Primary));
        var title = await primary.Title.FirstAsync();

        Assert.Equal("Install", title);
    }

    private sealed class CapturingDialog : IDialog
    {
        public IReadOnlyList<IOption> Options { get; private set; } = [];

        public Task<bool> Show<TViewModel>(
            Maybe<TViewModel> viewModel,
            Maybe<IObservable<string>> title,
            Func<Maybe<TViewModel>, ICloseable, IEnumerable<IOption>> optionsFactory,
            Maybe<object> icon = default,
            DialogTone tone = DialogTone.Neutral,
            DialogSize size = DialogSize.Auto)
        {
            Options = optionsFactory(viewModel, new CapturingCloseable()).ToList();
            return Task.FromResult(false);
        }
    }

    private sealed class CapturingCloseable : ICloseable
    {
        public void Close()
        {
        }

        public void Dismiss()
        {
        }
    }
}
