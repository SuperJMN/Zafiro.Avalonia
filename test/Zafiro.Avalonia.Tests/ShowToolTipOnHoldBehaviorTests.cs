using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using Zafiro.Avalonia.Behaviors;

namespace Zafiro.Avalonia.Tests;

public class ShowToolTipOnHoldBehaviorTests
{
    [AvaloniaFact]
    public void Hold_started_opens_tooltip_and_hold_completed_closes_it()
    {
        var button = new TestButton();
        var behavior = new ShowToolTipOnHoldBehavior();
        ToolTip.SetTip(button, "Details");

        Interaction.GetBehaviors(button).Add(behavior);

        behavior.HandleHoldingState(HoldingState.Started);

        Assert.True(ToolTip.GetIsOpen(button));

        behavior.HandleHoldingState(HoldingState.Completed);

        Assert.False(ToolTip.GetIsOpen(button));
    }

    [AvaloniaFact]
    public void Hold_started_cancels_next_button_activation()
    {
        var executedCount = 0;
        var button = new TestButton
        {
            Command = new TestCommand(() => executedCount++)
        };
        var behavior = new ShowToolTipOnHoldBehavior();
        ToolTip.SetTip(button, "Details");

        Interaction.GetBehaviors(button).Add(behavior);

        behavior.HandleHoldingState(HoldingState.Started);
        button.PerformClick();
        button.PerformClick();

        Assert.Equal(1, executedCount);
    }

    [AvaloniaFact]
    public void Attached_behavior_enables_hold_without_enabling_mouse_hold_by_default()
    {
        var button = new TestButton();

        Interaction.GetBehaviors(button).Add(new ShowToolTipOnHoldBehavior());

        Assert.True(InputElement.GetIsHoldingEnabled(button));
        Assert.False(InputElement.GetIsHoldWithMouseEnabled(button));
    }

    [AvaloniaFact]
    public void Hold_canceled_does_not_cancel_later_button_activation()
    {
        var executedCount = 0;
        var button = new TestButton
        {
            Command = new TestCommand(() => executedCount++)
        };
        var behavior = new ShowToolTipOnHoldBehavior();
        ToolTip.SetTip(button, "Details");

        Interaction.GetBehaviors(button).Add(behavior);

        behavior.HandleHoldingState(HoldingState.Started);
        behavior.HandleHoldingState(HoldingState.Canceled);
        button.PerformClick();

        Assert.Equal(1, executedCount);
    }

    private sealed class TestButton : Button
    {
        public void PerformClick()
        {
            OnClick();
        }
    }

    private sealed class TestCommand(System.Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
