using Zafiro.UI.Navigation;

namespace TestApp.Shell;

public class MainViewModel(INavigator navigator)
{
    public INavigator Navigator { get; } = navigator;
}