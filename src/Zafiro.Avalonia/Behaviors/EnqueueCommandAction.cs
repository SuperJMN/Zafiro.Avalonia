using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Windows.Input;
using Avalonia.Data;
using Avalonia.Xaml.Interactivity;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Behaviors;

/// <summary>
/// An action that enqueues a command execution into a named <see cref="CommandPool"/>
/// with bounded concurrency, instead of executing it immediately.
/// </summary>
/// <remarks>
/// Use this as a drop-in replacement for <c>InvokeCommandAction</c> when you want
/// to throttle concurrent executions across multiple items (e.g., in an ItemsControl template).
/// <para/>
/// Example:
/// <code>
/// &lt;Interaction.Behaviors&gt;
///     &lt;DataContextChangedTrigger&gt;
///         &lt;EnqueueCommandAction Command="{Binding LoadFullProject}"
///                                PoolName="ProjectDetails"
///                                MaxConcurrency="1"
///                                DelayBetweenSeconds="0.5" /&gt;
///     &lt;/DataContextChangedTrigger&gt;
/// &lt;/Interaction.Behaviors&gt;
/// </code>
/// </remarks>
public class EnqueueCommandAction : StyledElementAction
{
    /// <summary>
    /// Identifies the <see cref="Command"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<EnqueueCommandAction, ICommand?>(nameof(Command), defaultBindingMode: BindingMode.OneWay);

    /// <summary>
    /// Identifies the <see cref="CommandParameter"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<EnqueueCommandAction, object?>(nameof(CommandParameter), defaultBindingMode: BindingMode.OneWay);

    /// <summary>
    /// Identifies the <see cref="PoolName"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<string> PoolNameProperty =
        AvaloniaProperty.Register<EnqueueCommandAction, string>(nameof(PoolName), "Default");

    /// <summary>
    /// Identifies the <see cref="MaxConcurrency"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<int> MaxConcurrencyProperty =
        AvaloniaProperty.Register<EnqueueCommandAction, int>(nameof(MaxConcurrency), 3);

    /// <summary>
    /// Identifies the <see cref="DelayBetweenSeconds"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<double> DelayBetweenSecondsProperty =
        AvaloniaProperty.Register<EnqueueCommandAction, double>(nameof(DelayBetweenSeconds), 0);

    /// <summary>
    /// Identifies the <see cref="IsPoolExecuting"/> avalonia property.
    /// </summary>
    public static readonly DirectProperty<EnqueueCommandAction, bool> IsPoolExecutingProperty =
        AvaloniaProperty.RegisterDirect<EnqueueCommandAction, bool>(nameof(IsPoolExecuting), o => o.IsPoolExecuting);

    /// <summary>
    /// Identifies the <see cref="PoolExecutingCount"/> avalonia property.
    /// </summary>
    public static readonly DirectProperty<EnqueueCommandAction, int> PoolExecutingCountProperty =
        AvaloniaProperty.RegisterDirect<EnqueueCommandAction, int>(nameof(PoolExecutingCount), o => o.PoolExecutingCount);

    /// <summary>
    /// Identifies the <see cref="PoolPendingCount"/> avalonia property.
    /// </summary>
    public static readonly DirectProperty<EnqueueCommandAction, int> PoolPendingCountProperty =
        AvaloniaProperty.RegisterDirect<EnqueueCommandAction, int>(nameof(PoolPendingCount), o => o.PoolPendingCount);

    /// <summary>
    /// Identifies the <see cref="PoolTotalCount"/> avalonia property.
    /// </summary>
    public static readonly DirectProperty<EnqueueCommandAction, int> PoolTotalCountProperty =
        AvaloniaProperty.RegisterDirect<EnqueueCommandAction, int>(nameof(PoolTotalCount), o => o.PoolTotalCount);

    /// <summary>
    /// Identifies the <see cref="PoolCompletedCount"/> avalonia property.
    /// </summary>
    public static readonly DirectProperty<EnqueueCommandAction, int> PoolCompletedCountProperty =
        AvaloniaProperty.RegisterDirect<EnqueueCommandAction, int>(nameof(PoolCompletedCount), o => o.PoolCompletedCount);

    private readonly CompositeDisposable pendingJobs = new();

    private bool isPoolExecuting;
    private int poolCompletedCount;
    private int poolExecutingCount;
    private int poolPendingCount;

    private IDisposable? poolSubscription;
    private int poolTotalCount;

    /// <summary>
    /// Gets or sets the command to execute. This is an avalonia property.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter passed to the command. This is an avalonia property.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the name of the execution pool. All actions with the same pool name
    /// share the same concurrency limit. This is an avalonia property.
    /// </summary>
    public string PoolName
    {
        get => GetValue(PoolNameProperty);
        set => SetValue(PoolNameProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of concurrent command executions allowed in the pool.
    /// Only the first action to create a pool determines its concurrency.
    /// This is an avalonia property.
    /// </summary>
    public int MaxConcurrency
    {
        get => GetValue(MaxConcurrencyProperty);
        set => SetValue(MaxConcurrencyProperty, value);
    }

    /// <summary>
    /// Gets or sets the delay in seconds to wait after each command completes
    /// before starting the next one. Defaults to 0 (no delay).
    /// This is an avalonia property.
    /// </summary>
    public double DelayBetweenSeconds
    {
        get => GetValue(DelayBetweenSecondsProperty);
        set => SetValue(DelayBetweenSecondsProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the pool is currently executing any command.
    /// </summary>
    public bool IsPoolExecuting
    {
        get => isPoolExecuting;
        private set => SetAndRaise(IsPoolExecutingProperty, ref isPoolExecuting, value);
    }

    /// <summary>
    /// Gets the number of commands currently executing in the pool.
    /// </summary>
    public int PoolExecutingCount
    {
        get => poolExecutingCount;
        private set => SetAndRaise(PoolExecutingCountProperty, ref poolExecutingCount, value);
    }

    /// <summary>
    /// Gets the number of commands currently pending in the pool.
    /// </summary>
    public int PoolPendingCount
    {
        get => poolPendingCount;
        private set => SetAndRaise(PoolPendingCountProperty, ref poolPendingCount, value);
    }

    /// <summary>
    /// Gets the total number of commands registered in the pool.
    /// </summary>
    public int PoolTotalCount
    {
        get => poolTotalCount;
        private set => SetAndRaise(PoolTotalCountProperty, ref poolTotalCount, value);
    }

    /// <summary>
    /// Gets the number of commands completed in the pool.
    /// </summary>
    public int PoolCompletedCount
    {
        get => poolCompletedCount;
        private set => SetAndRaise(PoolCompletedCountProperty, ref poolCompletedCount, value);
    }


    /// <inheritdoc />
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
        poolSubscription?.Dispose();

        if (string.IsNullOrEmpty(poolName))
        {
            return;
        }

        var weakSelf = new WeakReference<EnqueueCommandAction>(this);

        poolSubscription = Observable.Return(CommandPool.Get(poolName))
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
                    self.PoolExecutingCount = tuple.Executing;
                    self.IsPoolExecuting = tuple.Executing > 0;
                    self.PoolPendingCount = tuple.Pending;
                    self.PoolTotalCount = tuple.Total;
                    self.PoolCompletedCount = tuple.Completed;
                }
            });
    }


    /// <inheritdoc />
    public override object? Execute(object? sender, object? parameter)
    {
        if (!IsEnabled)
        {
            return false;
        }

        var command = Command;
        if (command is null)
        {
            return false;
        }

        var pool = CommandPool.GetOrCreate(PoolName, MaxConcurrency, TimeSpan.FromSeconds(DelayBetweenSeconds));

        // Initial subscription if not already set up (e.g. if created via Execute without PropertyChanged trigger or default Name)
        if (poolSubscription == null)
        {
            UpdateSubscription(PoolName);
        }

        // Capture CommandParameter here on the UI thread — accessing AvaloniaProperties
        // from a background thread (e.g. after Observable.Timer) would throw.
        var commandParameter = CommandParameter;
        var work = CreateWork(command, commandParameter);

        var job = pool.Enqueue(work);

        if (sender is Visual visual)
        {
            var composite = new CompositeDisposable(job);
            Observable.FromEventPattern<VisualTreeAttachmentEventArgs>(
                    h => visual.DetachedFromVisualTree += h,
                    h => visual.DetachedFromVisualTree -= h)
                .Take(1)
                .Subscribe(_ => composite.Dispose())
                .DisposeWith(composite);

            pendingJobs.Add(composite);
        }
        else
        {
            pendingJobs.Add(job);
        }

        return true;
    }

    private static IObservable<Unit> CreateWork(ICommand command, object? commandParameter)
    {
        if (command is IEnhancedCommand enhanced)
        {
            return Observable.Create<Unit>(observer =>
            {
                var disposable = new CompositeDisposable();

                // Subscribe to IsExecuting BEFORE firing the command so we don't miss
                // the true→false transition. SkipWhile skips the initial replayed `false`,
                // then we wait for `false` after `true` = command completed.
                enhanced.IsExecuting
                    .SkipWhile(executing => !executing)
                    .Where(executing => !executing)
                    .Take(1)
                    .Subscribe(
                        _ =>
                        {
                            observer.OnNext(Unit.Default);
                            observer.OnCompleted();
                        },
                        observer.OnError)
                    .DisposeWith(disposable);

                // Fire the command on the UI thread (pool may subscribe from a timer thread).
                AvaloniaScheduler.Instance.Schedule(() =>
                {
                    if (command.CanExecute(commandParameter))
                    {
                        command.Execute(commandParameter);
                    }
                    else
                    {
                        // Command can't execute — complete the work so the pool moves on.
                        disposable.Dispose();
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    }
                }).DisposeWith(disposable);

                return disposable;
            });
        }

        // Fallback for plain ICommand: fire on UI thread and complete immediately.
        return Observable.Start(() =>
        {
            if (command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
        }, AvaloniaScheduler.Instance).Select(_ => Unit.Default);
    }
}