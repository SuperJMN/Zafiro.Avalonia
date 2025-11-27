using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using DynamicData;
using DynamicData.Binding;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls;

public sealed class SectionGroupView : IDisposable
{
    private readonly CompositeDisposable disposable = new();

    public SectionGroupView(IGroup<INavigationRoot, string, SectionGroup> group)
    {
        Group = group.Key;

        group.Cache.Connect()
            .AutoRefresh(section => section.SortOrder)
            .Sort(SortExpressionComparer<INavigationRoot>.Ascending(section => section.SortOrder))
            .Bind(out var sections)
            .Subscribe()
            .DisposeWith(disposable);

        Sections = sections;
    }

    public SectionGroup Group { get; }

    public ReadOnlyObservableCollection<INavigationRoot> Sections { get; }

    public void Dispose()
    {
        disposable.Dispose();
    }
}