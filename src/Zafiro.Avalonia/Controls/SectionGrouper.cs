using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using DynamicData;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls;

public sealed class SectionGrouper : IDisposable
{
    private readonly CompositeDisposable disposable = new();

    public SectionGrouper(IObservable<IChangeSet<ISection, string>> sectionChanges)
    {
        sectionChanges
            .AutoRefresh(section => section.IsVisible)
            .AutoRefresh(section => section.SortOrder)
            .AutoRefresh(section => section.Group)
            .Filter(section => section.IsVisible)
            .Group(section => section.Group)
            .Transform(group => new SectionGroupView(group))
            .DisposeMany()
            .Bind(out var groups)
            .Subscribe()
            .DisposeWith(disposable);

        SectionGroups = groups;
    }

    public ReadOnlyObservableCollection<SectionGroupView> SectionGroups { get; }

    public void Dispose()
    {
        disposable.Dispose();
    }
}