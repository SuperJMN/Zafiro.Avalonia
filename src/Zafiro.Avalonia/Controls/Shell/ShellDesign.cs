using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Zafiro.UI.Navigation;
using Zafiro.UI.Navigation.Sections;
using Zafiro.UI.Shell;

namespace Zafiro.Avalonia.Controls.Shell;

public class ShellDesign : IShell
{
    public void GoToSection(string sectionName)
    {
        throw new NotSupportedException();
    }

    public object Header { get; set; } = "Header that is too long to fit in the header";
    public IObservable<object?> ContentHeader { get; set; } = Observable.Return<object?>("Content Header");

    public IEnumerable<INavigationRoot> Sections =>
    [
        new SimpleSection { Name = "Hi Test section 1. Very long for the testing", Icon = new Icon() { Source = "fa-wallet", }, ContentValue = "Test" },
        new SimpleSection { Name = "Test section 2", Icon = new Icon() { Source = "fa-gear" } },
        new SimpleSection { Name = "Test section 3", Icon = new Icon() { Source = "fa-user" } }
    ];

    public INavigationRoot SelectedSection { get; set; }

    public INavigator Navigator { get; } = new Navigator(new ServiceCollection().BuildServiceProvider(), Maybe<ILogger>.None, null);
}