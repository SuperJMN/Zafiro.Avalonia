using System.Windows.Input;

namespace Zafiro.Avalonia.Controls.Navigation;

public interface IFrameBackParticipant
{
    IObservable<bool> CanHandleBack { get; }

    ICommand BackCommand { get; }
}
