using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Windows.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using JetBrains.Annotations;
using Zafiro.Avalonia.Controls.Navigation;

namespace Zafiro.Avalonia.Controls;

[PublicAPI]
public class MasterDetailsView : TemplatedControl, IFrameBackParticipant
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<MasterDetailsView, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<MasterDetailsView, object?>(nameof(SelectedItem));

    public static readonly DirectProperty<MasterDetailsView, ICommand> OpenDetailsCommandProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, ICommand>(nameof(OpenDetailsCommand), o => o.OpenDetailsCommand,
            (o, v) => o.OpenDetailsCommand = v);

    public static readonly DirectProperty<MasterDetailsView, ICommand> BackCommandProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, ICommand>(nameof(BackCommand), o => o.BackCommand,
            (o, v) => o.BackCommand = v);

    public static readonly DirectProperty<MasterDetailsView, ICommand> CloseDetailsCommandProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, ICommand>(nameof(CloseDetailsCommand), o => o.CloseDetailsCommand,
            (o, v) => o.CloseDetailsCommand = v);

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<IDataTemplate?> CompactItemTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IDataTemplate?>(nameof(CompactItemTemplate));

    public static readonly StyledProperty<IDataTemplate?> DetailsTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IDataTemplate?>(nameof(DetailsTemplate));

    public static readonly StyledProperty<IDataTemplate?> EmptyTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IDataTemplate?>(nameof(EmptyTemplate));

    public static readonly StyledProperty<IDataTemplate?> WideMasterTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IDataTemplate?>(nameof(WideMasterTemplate));

    public static readonly StyledProperty<IDataTemplate?> CompactMasterTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IDataTemplate?>(nameof(CompactMasterTemplate));

    public static readonly StyledProperty<double> CompactWidthProperty =
        AvaloniaProperty.Register<MasterDetailsView, double>(nameof(CompactWidth), 400);

    public static readonly StyledProperty<double> MasterPaneWidthProperty =
        AvaloniaProperty.Register<MasterDetailsView, double>(nameof(MasterPaneWidth), 200);

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MasterDetailsView, object?>(nameof(Header));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<MasterDetailsView, object?>(nameof(Footer));

    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IDataTemplate?>(nameof(FooterTemplate));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IDataTemplate?>(nameof(HeaderTemplate));

    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<MasterDetailsView, bool>(nameof(IsCompact));

    public static readonly StyledProperty<bool> AreDetailsShownProperty =
        AvaloniaProperty.Register<MasterDetailsView, bool>(nameof(AreDetailsShown));

    public static readonly StyledProperty<object?> NavigationKeyProperty =
        AvaloniaProperty.Register<MasterDetailsView, object?>(nameof(NavigationKey));

    public static readonly DirectProperty<MasterDetailsView, MasterDetailsViewContext> TemplateContextProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, MasterDetailsViewContext>(nameof(TemplateContext), o => o.TemplateContext);

    public static readonly DirectProperty<MasterDetailsView, bool> HasWideMasterTemplateProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, bool>(nameof(HasWideMasterTemplate), o => o.HasWideMasterTemplate);

    public static readonly DirectProperty<MasterDetailsView, bool> HasCompactMasterTemplateProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, bool>(nameof(HasCompactMasterTemplate), o => o.HasCompactMasterTemplate);

    public static readonly DirectProperty<MasterDetailsView, bool> HasEmptyTemplateProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, bool>(nameof(HasEmptyTemplate), o => o.HasEmptyTemplate);

    public static readonly DirectProperty<MasterDetailsView, IDataTemplate?> EffectiveCompactItemTemplateProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, IDataTemplate?>(nameof(EffectiveCompactItemTemplate), o => o.EffectiveCompactItemTemplate);

    public static readonly DirectProperty<MasterDetailsView, bool> HasSelectedItemProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, bool>(nameof(HasSelectedItem), o => o.HasSelectedItem);

    public static readonly DirectProperty<MasterDetailsView, bool> HasItemsProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, bool>(nameof(HasItems), o => o.HasItems);

    private readonly MasterDetailsViewContext templateContext;
    private CompositeDisposable? visualTreeSubscriptions;
    private SerialDisposable? itemsSourceSubscription;
    private ICommand backCommand = null!;
    private ICommand closeDetailsCommand = null!;
    private bool hasItems;
    private ICommand openDetailsCommand = null!;
    private bool navigationKeyInitialized;

    public MasterDetailsView()
    {
        templateContext = new MasterDetailsViewContext(this);

        CanHandleBack = Observable.Defer(() => this.GetObservable(IsCompactProperty)
            .StartWith(IsCompact)
            .CombineLatest(this.GetObservable(AreDetailsShownProperty).StartWith(AreDetailsShown), (isCompact, areDetailsShown) => isCompact && areDetailsShown)
            .DistinctUntilChanged());

        CloseDetailsCommand = new DelegateCommand(_ => CloseDetails());
        BackCommand = CloseDetailsCommand;
        OpenDetailsCommand = new DelegateCommand(OpenDetails);
    }

    public IObservable<bool> CanHandleBack { get; }

    public MasterDetailsViewContext TemplateContext => templateContext;

    public bool HasWideMasterTemplate => WideMasterTemplate is not null;

    public bool HasCompactMasterTemplate => CompactMasterTemplate is not null;

    public bool HasEmptyTemplate => EmptyTemplate is not null;

    public IDataTemplate? EffectiveCompactItemTemplate => CompactItemTemplate ?? ItemTemplate;

    public bool HasSelectedItem => SelectedItem is not null;

    public bool HasItems
    {
        get => hasItems;
        private set => SetAndRaise(HasItemsProperty, ref hasItems, value);
    }

    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public double MasterPaneWidth
    {
        get => GetValue(MasterPaneWidthProperty);
        set => SetValue(MasterPaneWidthProperty, value);
    }

    public double CompactWidth
    {
        get => GetValue(CompactWidthProperty);
        set => SetValue(CompactWidthProperty, value);
    }

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? DetailsTemplate
    {
        get => GetValue(DetailsTemplateProperty);
        set => SetValue(DetailsTemplateProperty, value);
    }

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? CompactItemTemplate
    {
        get => GetValue(CompactItemTemplateProperty);
        set => SetValue(CompactItemTemplateProperty, value);
    }

    public IDataTemplate? EmptyTemplate
    {
        get => GetValue(EmptyTemplateProperty);
        set => SetValue(EmptyTemplateProperty, value);
    }

    public IDataTemplate? WideMasterTemplate
    {
        get => GetValue(WideMasterTemplateProperty);
        set => SetValue(WideMasterTemplateProperty, value);
    }

    public IDataTemplate? CompactMasterTemplate
    {
        get => GetValue(CompactMasterTemplateProperty);
        set => SetValue(CompactMasterTemplateProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public bool AreDetailsShown
    {
        get => GetValue(AreDetailsShownProperty);
        set => SetValue(AreDetailsShownProperty, value);
    }

    public object? NavigationKey
    {
        get => GetValue(NavigationKeyProperty);
        set => SetValue(NavigationKeyProperty, value);
    }

    public ICommand OpenDetailsCommand
    {
        get => openDetailsCommand;
        private set => SetAndRaise(OpenDetailsCommandProperty, ref openDetailsCommand, value);
    }

    public ICommand BackCommand
    {
        get => backCommand;
        private set => SetAndRaise(BackCommandProperty, ref backCommand, value);
    }

    public ICommand CloseDetailsCommand
    {
        get => closeDetailsCommand;
        private set => SetAndRaise(CloseDetailsCommandProperty, ref closeDetailsCommand, value);
    }

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        visualTreeSubscriptions = new CompositeDisposable();
        itemsSourceSubscription = new SerialDisposable().DisposeWith(visualTreeSubscriptions);

        (this.FindAncestorOfType<Frame>()?.RegisterBackParticipant(this) ?? Disposable.Empty)
            .DisposeWith(visualTreeSubscriptions);

        this.GetObservable(BoundsProperty)
            .StartWith(Bounds)
            .CombineLatest(this.GetObservable(CompactWidthProperty).StartWith(CompactWidth), (bounds, compactWidth) => bounds.Width > 0 && bounds.Width < compactWidth)
            .DistinctUntilChanged()
            .Do(isCompact => IsCompact = isCompact)
            .Subscribe()
            .DisposeWith(visualTreeSubscriptions);

        TrackItemsSourceChanges();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        visualTreeSubscriptions?.Dispose();
        visualTreeSubscriptions = null;
        itemsSourceSubscription = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == NavigationKeyProperty)
        {
            if (navigationKeyInitialized && !Equals(change.OldValue, change.NewValue))
            {
                CloseDetails();
            }

            navigationKeyInitialized = true;
        }

        if (change.Property == ItemsSourceProperty)
        {
            TrackItemsSourceChanges();
        }

        if (change.Property == SelectedItemProperty)
        {
            RaisePropertyChanged(HasSelectedItemProperty, change.OldValue is not null, HasSelectedItem);

            if (SelectedItem is null)
            {
                CloseDetails();
            }
        }

        if (change.Property == ItemTemplateProperty || change.Property == CompactItemTemplateProperty)
        {
            RaisePropertyChanged(EffectiveCompactItemTemplateProperty, (IDataTemplate?)change.OldValue, EffectiveCompactItemTemplate);
        }

        if (change.Property == WideMasterTemplateProperty)
        {
            RaisePropertyChanged(HasWideMasterTemplateProperty, change.OldValue is not null, HasWideMasterTemplate);
        }

        if (change.Property == CompactMasterTemplateProperty)
        {
            RaisePropertyChanged(HasCompactMasterTemplateProperty, change.OldValue is not null, HasCompactMasterTemplate);
        }

        if (change.Property == EmptyTemplateProperty)
        {
            RaisePropertyChanged(HasEmptyTemplateProperty, change.OldValue is not null, HasEmptyTemplate);
        }
    }

    private void OpenDetails(object? item)
    {
        var itemToOpen = item ?? SelectedItem;

        if (itemToOpen is null)
        {
            return;
        }

        SelectedItem = itemToOpen;
        AreDetailsShown = true;
    }

    private void CloseDetails()
    {
        AreDetailsShown = false;
    }

    private void TrackItemsSourceChanges()
    {
        if (itemsSourceSubscription is null)
        {
            RefreshItemsState();
            return;
        }

        itemsSourceSubscription.Disposable = ItemsSource is INotifyCollectionChanged collectionChanged
            ? Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    handler => collectionChanged.CollectionChanged += handler,
                    handler => collectionChanged.CollectionChanged -= handler)
                .Do(_ => RefreshItemsState())
                .Subscribe()
            : Disposable.Empty;

        RefreshItemsState();
    }

    private void RefreshItemsState()
    {
        HasItems = ItemsSource?.Cast<object?>().Any() == true;
        ClearSelectionIfItemLeftSource();
    }

    private void ClearSelectionIfItemLeftSource()
    {
        if (SelectedItem is null || ItemsSource is null)
        {
            return;
        }

        if (ItemsSource.Cast<object?>().Contains(SelectedItem))
        {
            return;
        }

        SelectedItem = null;
        CloseDetails();
    }

    private sealed class DelegateCommand(Action<object?> execute) : ICommand
    {
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
