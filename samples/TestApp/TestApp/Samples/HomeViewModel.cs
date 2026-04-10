using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using Zafiro.UI.Navigation;

namespace TestApp.Samples;

public partial class HomeViewModel : ReactiveObject
{
    private readonly SourceCache<SampleCard, string> allSamples = new(c => c.Name);
    private readonly ReadOnlyObservableCollection<SampleCard> filteredSamples;

    public HomeViewModel(IEnumerable<SampleCard> samples, INavigator navigator, HomeViewState state)
    {
        State = state;
        allSamples.AddOrUpdate(samples);

        Categories = new[] { "All" }
            .Concat(samples.Select(s => s.Category).Distinct().OrderBy(c => c))
            .ToList();

        var searchFilter = this.WhenAnyValue(x => x.State.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(300), RxSchedulers.MainThreadScheduler)
            .Select(BuildSearchFilter);

        var categoryFilter = this.WhenAnyValue(x => x.State.SelectedCategory)
            .Select(BuildCategoryFilter);

        var combinedFilter = searchFilter
            .CombineLatest(categoryFilter, (sf, cf) => new Func<SampleCard, bool>(card => sf(card) && cf(card)));

        allSamples
            .Connect()
            .Filter(combinedFilter)
            .SortBy(c => c.Name)
            .Bind(out filteredSamples)
            .Subscribe();

        NavigateToSample = ReactiveCommand.CreateFromTask<SampleCard>(async card => { await navigator.Go(card.ViewModelType); });
    }

    public HomeViewState State { get; }

    public ReadOnlyObservableCollection<SampleCard> FilteredSamples => filteredSamples;
    public IReadOnlyList<string> Categories { get; }
    public ReactiveCommand<SampleCard, Unit> NavigateToSample { get; }

    private static Func<SampleCard, bool> BuildSearchFilter(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return _ => true;
        }

        return card => card.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
                       || card.Description.Contains(text, StringComparison.OrdinalIgnoreCase);
    }

    private static Func<SampleCard, bool> BuildCategoryFilter(string? category)
    {
        if (string.IsNullOrWhiteSpace(category) || string.Equals(category, "All", StringComparison.OrdinalIgnoreCase))
        {
            return _ => true;
        }

        return card => card.Category.Equals(category, StringComparison.OrdinalIgnoreCase);
    }
}