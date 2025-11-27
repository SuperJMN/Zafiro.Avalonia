using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using DynamicData;
using DynamicData.Binding;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls;

public sealed class SectionGrouper : IDisposable
{
    private readonly CompositeDisposable disposable = new();

    public SectionGrouper(IObservable<IChangeSet<INavigationRoot, string>> sectionChanges)
    {
        sectionChanges
            .AutoRefresh(section => section.IsVisible)
            .AutoRefresh(section => section.SortOrder)
            .AutoRefresh(section => section.Group)
            .Filter(section => section.IsVisible)
            .Group(section => section.Group)
            .Transform(group => new SectionGroupView(group))
            .DisposeMany()
            .SortAndBind(out var groups, SortExpressionComparer<SectionGroupView>.Ascending(w => w.Group.SortOrder))
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