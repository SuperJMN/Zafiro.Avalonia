
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Dialogs;

public interface IOption<T, Q> : IOption
{
    IEnhancedCommand<T, Q> TypedCommand { get; }
}

public interface IOption
{
    string Title { get; }

    IEnhancedCommand Command { get; }

    bool IsDefault { get; }

    bool IsCancel { get; }
    public IObservable<bool> IsVisible { get; }

    public OptionRole Role { get; }
}

public enum OptionRole
{
    /// <summary>
    /// Primary action (e.g. "OK", "Save", "Accept"…)
    /// </summary>
    Primary,

    /// <summary>
    /// Secondary action, complementary to the primary one (e.g. "Advanced options", "More info"…)
    /// </summary>
    Secondary,

    /// <summary>
    /// Cancel or close action (e.g. "Cancel", "Close"...)
    /// </summary>
    Cancel,

    /// <summary>
    /// Destructive action (e.g. "Delete", "Remove")
    /// </summary>
    Destructive,

    /// <summary>
    /// Informational or help action, or simply one that is not central
    /// (e.g. "Help", "Read more", "View documentation")
    /// </summary>
    Info
}
