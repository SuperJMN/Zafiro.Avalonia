using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Subjects;

namespace Zafiro.Avalonia.Behaviors;

/// <summary>
/// A named execution pool that limits the number of concurrent command executions
/// and optionally inserts a cooldown delay between them.
/// All <see cref="InvokeCommandRequestAction"/> instances with the same <c>PoolName</c>
/// share a single pool.
/// </summary>
public sealed class CommandPool : IDisposable
{
    private static readonly ConcurrentDictionary<string, CommandPool> Pools = new();
    private readonly TimeSpan delayBetween;

    private readonly Subject<IObservable<Unit>> queue = new();
    private readonly IDisposable subscription;
    private int refCount;

    private CommandPool(int maxConcurrency, TimeSpan delayBetween)
    {
        this.delayBetween = delayBetween;

        subscription = queue
            .Select(work => delayBetween > TimeSpan.Zero
                ? work.Concat(Observable.Timer(delayBetween).Select(_ => Unit.Default))
                : work)
            .Merge(maxConcurrency)
            .Subscribe();
    }

    public void Dispose()
    {
        queue.OnCompleted();
        subscription.Dispose();
        queue.Dispose();
    }

    /// <summary>
    /// Gets or creates a pool with the specified name, max concurrency and inter-execution delay.
    /// If the pool already exists, the existing instance is returned (parameters are not changed).
    /// </summary>
    public static CommandPool GetOrCreate(string poolName, int maxConcurrency, TimeSpan delayBetween)
    {
        return Pools.GetOrAdd(poolName, _ => new CommandPool(maxConcurrency, delayBetween));
    }

    /// <summary>
    /// Enqueues a cold observable for execution within this pool's concurrency limit.
    /// </summary>
    public void Enqueue(IObservable<Unit> work)
    {
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