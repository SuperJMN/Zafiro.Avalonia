using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using DynamicData;
using DynamicData.Binding;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls;

public sealed class SectionGroupView : IDisposable
{
    private readonly CompositeDisposable disposable = new();

    public SectionGroupView(IGroup<ISection, string, SectionGroup> group)
    {
        Group = group.GroupKey;

        group.Cache.Connect()
            .AutoRefresh(section => section.SortOrder)
            .Sort(SortExpressionComparer<ISection>.Ascending(section => section.SortOrder))
            .Bind(out ReadOnlyObservableCollection<ISection> sections)
            .Subscribe()
            .DisposeWith(disposable);

        Sections = sections;
    }

    public SectionGroup Group { get; }

    public ReadOnlyObservableCollection<ISection> Sections { get; }

    public void Dispose()
    {
        disposable.Dispose();
    }
}
