using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.SourceGenerators;
using Serilog;
using Zafiro.UI.Navigation;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls.Shell;

public partial class SimpleSection : ReactiveObject, INavigationRoot
{
    [Reactive] private object? icon;
    [Reactive] private bool isVisible = true;
    [Reactive] private INavigator navigator;
    [Reactive] private int sortOrder;

    public SimpleSection() : this(new Navigator(new ServiceCollection().BuildServiceProvider(), Maybe<ILogger>.None, null))
    {
    }

    public SimpleSection(INavigator navigator)
    {
        Navigator = navigator;
    }

    public object ContentValue { get; set; } = new();

    public string Name { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public IObservable<object> Content => Observable.Return(ContentValue);
    public SectionGroup Group { get; set; } = new();
}