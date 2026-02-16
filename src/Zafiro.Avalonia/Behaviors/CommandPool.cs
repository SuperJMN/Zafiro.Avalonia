using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Subjects;

namespace Zafiro.Avalonia.Behaviors;

/// <summary>
/// A named execution pool that limits the number of concurrent command executions
/// and optionally inserts a cooldown delay between them.
/// All <see cref="EnqueueCommandAction"/> instances with the same <c>PoolName</c>
/// share a single pool.
/// </summary>
public sealed class CommandPool : IDisposable
{
    private static readonly ConcurrentDictionary<string, CommandPool> Pools = new();
    private static readonly Subject<CommandPool> PoolCreatedSubject = new();
    private readonly BehaviorSubject<int> completedCount = new(0);

    private readonly TimeSpan delayBetween;
    private readonly BehaviorSubject<int> executingCount = new(0);
    private readonly BehaviorSubject<int> pendingCount = new(0);

    private readonly Subject<IObservable<Unit>> queue = new();

    private readonly IDisposable subscription;
    private readonly BehaviorSubject<int> totalCount = new(0);
    private int refCount;

    private CommandPool(string name, int maxConcurrency, TimeSpan delayBetween)
    {
        Name = name;
        this.delayBetween = delayBetween;

        subscription = queue
            .Select(work =>
            {
                var trackedWork = Observable.Create<Unit>(observer =>
                {
                    UpdateExecuting(1);
                    UpdatePending(-1);
                    return work
                        .Do(_ => { }, () =>
                        {
                            UpdateExecuting(-1);
                            UpdateCompleted(1);
                        })
                        .Subscribe(observer);
                });

                return delayBetween > TimeSpan.Zero
                    ? trackedWork.Concat(Observable.Timer(delayBetween).Select(_ => Unit.Default))
                    : trackedWork;
            })
            .Merge(maxConcurrency)
            .Subscribe();
    }

    public string Name { get; }

    public int ExecutingCount => executingCount.Value;
    public int PendingCount => pendingCount.Value;
    public int TotalCount => totalCount.Value;
    public int CompletedCount => completedCount.Value;

    public bool IsExecuting => ExecutingCount > 0;

    public IObservable<int> ExecutingCountObservable => executingCount.AsObservable();
    public IObservable<int> PendingCountObservable => pendingCount.AsObservable();
    public IObservable<int> TotalCountObservable => totalCount.AsObservable();
    public IObservable<int> CompletedCountObservable => completedCount.AsObservable();

    public IObservable<bool> IsExecutingObservable => executingCount.Select(x => x > 0).DistinctUntilChanged();

    public static IObservable<CommandPool> PoolCreated => PoolCreatedSubject.AsObservable();

    public void Dispose()
    {
        queue.OnCompleted();
        subscription.Dispose();
        queue.Dispose();
        executingCount.Dispose();
        pendingCount.Dispose();
        totalCount.Dispose();
        completedCount.Dispose();
    }

    private void UpdateExecuting(int delta)
    {
        lock (executingCount)
        {
            var newCount = executingCount.Value + delta;
            executingCount.OnNext(newCount);
        }
    }

    private void UpdatePending(int delta)
    {
        lock (pendingCount)
        {
            var newCount = pendingCount.Value + delta;
            pendingCount.OnNext(newCount);
        }
    }

    private void UpdateTotal(int delta)
    {
        lock (totalCount)
        {
            var newCount = totalCount.Value + delta;
            totalCount.OnNext(newCount);
        }
    }

    private void UpdateCompleted(int delta)
    {
        lock (completedCount)
        {
            var newCount = completedCount.Value + delta;
            completedCount.OnNext(newCount);
        }
    }

    /// <summary>
    /// Gets or creates a pool with the specified name, max concurrency and inter-execution delay.
    /// If the pool already exists, the existing instance is returned (parameters are not changed).
    /// </summary>
    public static CommandPool GetOrCreate(string poolName, int maxConcurrency, TimeSpan delayBetween)
    {
        if (Pools.TryGetValue(poolName, out var existing))
        {
            return existing;
        }

        var newPool = new CommandPool(poolName, maxConcurrency, delayBetween);
        if (Pools.TryAdd(poolName, newPool))
        {
            PoolCreatedSubject.OnNext(newPool);
            return newPool;
        }

        newPool.Dispose();
        return Pools[poolName];
    }

    /// <summary>
    /// Gets the pool with the specified name, if it exists.
    /// </summary>
    public static CommandPool? Get(string poolName)
    {
        if (Pools.TryGetValue(poolName, out var pool))
        {
            return pool;
        }

        return null;
    }

    /// <summary>
    /// Enqueues a cold observable for execution within this pool's concurrency limit.
    /// </summary>
    public void Enqueue(IObservable<Unit> work)
    {
        UpdateTotal(1);
        UpdatePending(1);
        queue.OnNext(work);
    }

    /// <summary>
    /// Increments the reference count for this pool.
    /// </summary>
    internal void AddRef()
    {
        Interlocked.Increment(ref refCount);
    }

    /// <summary>
    /// Decrements the reference count and removes the pool from the registry when it reaches zero.
    /// </summary>
    internal void Release(string poolName)
    {
        if (Interlocked.Decrement(ref refCount) <= 0)
        {
            if (Pools.TryRemove(poolName, out var removed))
            {
                removed.Dispose();
            }
        }
    }
}