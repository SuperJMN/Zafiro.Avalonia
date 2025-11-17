using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using DynamicData;
using DynamicData.Binding;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls;

public sealed class SectionSorter : IDisposable
{
    private readonly CompositeDisposable disposable = new();

    public SectionSorter(IObservable<IChangeSet<ISection, string>> sectionChanges)
    {
        sectionChanges
            .AutoRefresh(w => w.IsVisible)
            .AutoRefresh(w => w.SortOrder)
            .Filter(s => s.IsVisible)
            .DisposeMany()
            .SortAndBind(out var filtered, SortExpressionComparer<ISection>.Ascending(w => w.SortOrder))
            .Subscribe()
            .DisposeWith(disposable);

        Sections = filtered;
    }

    public ReadOnlyObservableCollection<ISection> Sections { get; }

    public void Dispose()
    {
        disposable.Dispose();
    }
}