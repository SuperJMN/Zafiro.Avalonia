using ReactiveUI.SourceGenerators;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls.Shell;

public partial class SimpleSection : ReactiveObject, ISection
{
    [Reactive] private object? icon;
    [Reactive] private int sortOrder;
    public object ContentValue { get; set; }

    public string Name { get; set; }
    public string FriendlyName { get; set; }
    public bool IsVisible { get; set; } = true;
    public IObservable<object> Content => Observable.Return(ContentValue);
    public SectionGroup Group { get; set; } = new();
}