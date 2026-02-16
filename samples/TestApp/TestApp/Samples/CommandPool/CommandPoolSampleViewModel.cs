using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.CommandPool;

[Section(icon: "mdi-pool", sortIndex: 10)]
[SectionGroup("Samples")]
public class CommandPoolSampleViewModel : ReactiveObject
{
    public CommandPoolSampleViewModel()
    {
        Jobs = new ObservableCollection<JobViewModel>();
        for (var i = 0; i < 20; i++)
        {
            Jobs.Add(new JobViewModel($"Job {i + 1}"));
        }

        StartAll = ReactiveCommand.Create(() =>
        {
            var pool = Zafiro.Avalonia.Behaviors.CommandPool.GetOrCreate("SamplePool", 3, TimeSpan.FromSeconds(0.2));
            foreach (var job in Jobs)
            {
                pool.Enqueue(job.Work.Execute());
            }
        });
    }

    public ObservableCollection<JobViewModel> Jobs { get; }
    public ReactiveCommand<Unit, Unit> StartAll { get; }
}

public class JobViewModel : ReactiveObject
{
    private bool isExecuting;

    public JobViewModel(string name)
    {
        Name = name;
        Work = ReactiveCommand.CreateFromObservable(DoWork);
        Work.IsExecuting.BindTo(this, x => x.IsExecuting);
    }

    public string Name { get; }
    public ReactiveCommand<Unit, Unit> Work { get; }

    public bool IsExecuting
    {
        get => isExecuting;
        set => this.RaiseAndSetIfChanged(ref isExecuting, value);
    }

    private IObservable<Unit> DoWork()
    {
        return Observable.Timer(TimeSpan.FromSeconds(2))
            .Select(_ => Unit.Default);
    }
}