using Zafiro.Avalonia.Behaviors;

namespace Zafiro.Avalonia.Controls;

public class CommandPoolMonitor : Control
{
    public static readonly StyledProperty<string> PoolNameProperty =
        AvaloniaProperty.Register<CommandPoolMonitor, string>(nameof(PoolName), "Default");

    public static readonly DirectProperty<CommandPoolMonitor, bool> IsExecutingProperty =
        AvaloniaProperty.RegisterDirect<CommandPoolMonitor, bool>(nameof(IsExecuting), o => o.IsExecuting);

    public static readonly DirectProperty<CommandPoolMonitor, int> ExecutingCountProperty =
        AvaloniaProperty.RegisterDirect<CommandPoolMonitor, int>(nameof(ExecutingCount), o => o.ExecutingCount);

    public static readonly DirectProperty<CommandPoolMonitor, int> PendingCountProperty =
        AvaloniaProperty.RegisterDirect<CommandPoolMonitor, int>(nameof(PendingCount), o => o.PendingCount);

    public static readonly DirectProperty<CommandPoolMonitor, int> TotalCountProperty =
        AvaloniaProperty.RegisterDirect<CommandPoolMonitor, int>(nameof(TotalCount), o => o.TotalCount);

    public static readonly DirectProperty<CommandPoolMonitor, int> CompletedCountProperty =
        AvaloniaProperty.RegisterDirect<CommandPoolMonitor, int>(nameof(CompletedCount), o => o.CompletedCount);

    private int completedCount;
    private int executingCount;
    private bool isExecuting;
    private int pendingCount;

    private IDisposable? subscription;
    private int totalCount;

    public string PoolName
    {
        get => GetValue(PoolNameProperty);
        set => SetValue(PoolNameProperty, value);
    }

    public bool IsExecuting
    {
        get => isExecuting;
        private set => SetAndRaise(IsExecutingProperty, ref isExecuting, value);
    }

    public int ExecutingCount
    {
        get => executingCount;
        private set => SetAndRaise(ExecutingCountProperty, ref executingCount, value);
    }

    public int PendingCount
    {
        get => pendingCount;
        private set => SetAndRaise(PendingCountProperty, ref pendingCount, value);
    }

    public int TotalCount
    {
        get => totalCount;
        private set => SetAndRaise(TotalCountProperty, ref totalCount, value);
    }

    public int CompletedCount
    {
        get => completedCount;
        private set => SetAndRaise(CompletedCountProperty, ref completedCount, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PoolNameProperty)
        {
            UpdateSubscription(change.GetNewValue<string>());
        }
    }

    private void UpdateSubscription(string? poolName)
    {
        subscription?.Dispose();

        if (string.IsNullOrEmpty(poolName))
        {
            return;
        }

        var weakSelf = new WeakReference<CommandPoolMonitor>(this);

        subscription = Observable.Return(CommandPool.Get(poolName))
            .Where(p => p != null)
            .Merge(CommandPool.PoolCreated.Where(p => p.Name == poolName))
            .Take(1)
            .SelectMany(pool =>
            {
                if (pool is null)
                {
                    return Observable.Empty<(int Executing, int Pending, int Total, int Completed)>();
                }

                return Observable.CombineLatest(
                    pool.ExecutingCountObservable,
                    pool.PendingCountObservable,
                    pool.TotalCountObservable,
                    pool.CompletedCountObservable,
                    (executing, pending, total, completed) => (Executing: executing, Pending: pending, Total: total, Completed: completed));
            })
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(tuple =>
            {
                if (weakSelf.TryGetTarget(out var self))
                {
                    self.ExecutingCount = tuple.Executing;
                    self.IsExecuting = tuple.Executing > 0;
                    self.PendingCount = tuple.Pending;
                    self.TotalCount = tuple.Total;
                    self.CompletedCount = tuple.Completed;
                }
            });
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (subscription == null)
        {
            UpdateSubscription(PoolName);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        subscription?.Dispose();
    }
}