using System.Reactive.Disposables;
using ReactiveUI.SourceGenerators;
using Zafiro.UI.Navigation;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls.Shell;

public partial class SimpleSection : ReactiveObject, ISection
{
    private readonly CompositeDisposable disposable = new();
    [Reactive] private object? content;
    [Reactive] private object? icon;
    [Reactive] private bool isVisible = true;
    [Reactive] private INavigator navigator;
    [Reactive] private int sortOrder;

    public SimpleSection()
    {
        this.WhenAnyValue(x => x.Content)
            .WhereNotNull()
            .Select(o => new SimpleNavigator(o))
            .StartWith(new SimpleNavigator(new object()))
            .BindTo(this, x => x.Navigator)
            .DisposeWith(disposable);
    }

    public string Name { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public SectionGroup Group { get; set; } = new();

    public void Dispose()
    {
        Navigator?.Dispose();
    }
}