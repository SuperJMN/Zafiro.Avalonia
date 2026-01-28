using System.Reactive.Linq;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Dialogs;

public class Option<T, Q> : IOption<T, Q>
{
    public Option(IObservable<string> title, IEnhancedCommand<T, Q> command, Settings settings)
    {
        Title = title;
        Command = command;
        IsDefault = settings.IsDefault;
        IsCancel = settings.IsCancel;
        IsVisible = settings.IsVisible;
        Role = settings.Role;
    }

    public Option(string title, IEnhancedCommand<T, Q> command, Settings settings) : this(Observable.Return(title), command, settings)
    {
    }

    public IObservable<string> Title { get; }
    public IEnhancedCommand Command { get; }
    public bool IsDefault { get; }
    public bool IsCancel { get; }
    public IObservable<bool> IsVisible { get; }
    public OptionRole Role { get; }
    public IEnhancedCommand<T, Q> TypedCommand => (IEnhancedCommand<T, Q>)Command;
}

public class Option : IOption
{
    public Option(IObservable<string> title, IEnhancedCommand command, Settings settings)
    {
        Title = title;
        Command = command;
        IsDefault = settings.IsDefault;
        IsCancel = settings.IsCancel;
        IsVisible = settings.IsVisible;
        Role = settings.Role;
    }

    public Option(string title, IEnhancedCommand command, Settings settings) : this(Observable.Return(title), command, settings)
    {
    }

    public IObservable<string> Title { get; }
    public IEnhancedCommand Command { get; }
    public bool IsDefault { get; }
    public bool IsCancel { get; }
    public IObservable<bool> IsVisible { get; }
    public OptionRole Role { get; }
}