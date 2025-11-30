using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Reactive.Bindings;
using Serilog;
using Zafiro.UI.Navigation;
using Zafiro.UI.Navigation.Sections;
using Zafiro.UI.Shell;

namespace Zafiro.Avalonia.Controls.Shell;

public class ShellDesign : IShell
{
    public INavigator Navigator { get; } = new Navigator(new ServiceCollection().BuildServiceProvider(), Maybe<ILogger>.None, null);

    public void GoToSection(string sectionName)
    {
        throw new NotSupportedException();
    }

    public object Header { get; set; } = "Header that is too long to fit in the header";
    public ReadOnlyReactiveProperty<object?> ContentHeader { get; } = new ReadOnlyReactiveProperty<object?>(Observable.Return("Content"));

    public IEnumerable<ISection> Sections =>
    [
        new SimpleSection() { Name = "Hi Test section 1. Very long for the testing", Icon = new Icon() { Source = "fa-wallet", } },
        new SimpleSection() { Name = "Test section 2", Icon = new Icon() { Source = "fa-gear" } },
        new SimpleSection() { Name = "Test section 3", Icon = new Icon() { Source = "fa-user" } }
    ];

    public global::Reactive.Bindings.ReactiveProperty<ISection> SelectedSection { get; } = new();
}