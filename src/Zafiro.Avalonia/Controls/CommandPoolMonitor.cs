using Zafiro.Avalonia.Behaviors;
using Zafiro.Reactive;

namespace Zafiro.Avalonia.Controls;

public class CommandPoolMonitor : ReactiveObject
{
    private string? poolName;

    public CommandPoolMonitor()
    {
        var pool = this.WhenAnyValue(x => x.PoolName)
            .WhereNotNull()
            .Select(name =>
            {
                var existing = Observable.Return(CommandPool.Get(name)).WhereNotNull();
                var created = CommandPool.PoolCreated.Where(p => p.Name == name);

                return existing.Merge(created).Take(1);
            })
            .Switch()
            .ReplayLastActive();

        IsExecuting = pool.Select(x => x.IsExecutingObservable).Switch().DistinctUntilChanged().ReplayLastActive();
        ExecutingCount = pool.Select(x => x.ExecutingCountObservable).Switch().DistinctUntilChanged().ReplayLastActive();
        PendingCount = pool.Select(x => x.PendingCountObservable).Switch().DistinctUntilChanged().ReplayLastActive();
        TotalCount = pool.Select(x => x.TotalCountObservable).Switch().DistinctUntilChanged().ReplayLastActive();
        CompletedCount = pool.Select(x => x.CompletedCountObservable).Switch().DistinctUntilChanged().ReplayLastActive();
    }

    public string? PoolName
    {
        get => poolName;
        set => this.RaiseAndSetIfChanged(ref poolName, value);
    }

    public IObservable<bool> IsExecuting { get; }
    public IObservable<int> ExecutingCount { get; }
    public IObservable<int> PendingCount { get; }
    public IObservable<int> TotalCount { get; }
    public IObservable<int> CompletedCount { get; }
}