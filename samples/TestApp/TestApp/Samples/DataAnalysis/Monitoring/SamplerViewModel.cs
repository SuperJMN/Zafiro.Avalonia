using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.DataAnalysis.Monitoring;

[Section(icon: "mdi-chart-line")]
[SectionGroup("data-viz", "Data Visualization")]
public class SamplerViewModel
{
    public SamplerViewModel()
    {
        var dataSource = DataSource();
        var ticker = Observable.Interval(TimeSpan.FromSeconds(1)).Select(l => Unit.Default);
        var monitor = new Monitor(dataSource, ticker);
        Values = monitor.ValueCollection;
    }

    public ReadOnlyObservableCollection<double> Values { get; }

    private static IObservable<double> DataSource()
    {
        double previousValue = 0;
        var dataSource = Observable.Generate(0d,
            _ => true,
            i =>
            {
                var newValue = previousValue + (Random.Shared.NextDouble() * 2 - 1);
                previousValue = newValue;
                return newValue;
            },
            i => i * 100,
            _ => TimeSpan.FromMilliseconds(1000));

        return dataSource;
    }
}

public class Monitor
{
    public Monitor(IObservable<double> dataSource, IObservable<Unit> sampler)
    {
        DataSource = dataSource;
        Sampler = sampler;
        Values = Sampler.WithLatestFrom(DataSource, (_, d) => d);
        Values.ToObservableChangeSet().Bind(out var valueList).Subscribe();
        ValueCollection = valueList;
    }

    public IObservable<double> DataSource { get; }
    public IObservable<Unit> Sampler { get; }

    public ReadOnlyObservableCollection<double> ValueCollection { get; }

    public IObservable<double> Values { get; set; }
}