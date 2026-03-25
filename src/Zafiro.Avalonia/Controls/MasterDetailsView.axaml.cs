using System.Collections;
using System.Windows.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using JetBrains.Annotations;

namespace Zafiro.Avalonia.Controls;

[PublicAPI]
public class MasterDetailsView : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<MasterDetailsView, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<object> SelectedItemProperty =
        AvaloniaProperty.Register<MasterDetailsView, object>(nameof(SelectedItem));

    public static readonly DirectProperty<MasterDetailsView, ICommand> GoToDetailsProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, ICommand>(nameof(GoToDetails), o => o.GoToDetails,
            (o, v) => o.GoToDetails = v);

    public static readonly DirectProperty<MasterDetailsView, ICommand> BackCommandProperty =
        AvaloniaProperty.RegisterDirect<MasterDetailsView, ICommand>(nameof(BackCommand), o => o.BackCommand,
            (o, v) => o.BackCommand = v);

    public static readonly StyledProperty<bool> IsBackButtonEnabledProperty =
        AvaloniaProperty.Register<MasterDetailsView, bool>(nameof(IsBackButtonDisplayed), true);

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<IDataTemplate?> DetailsTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IDataTemplate?>(nameof(DetailsTemplate));

    public static readonly StyledProperty<double> CompactWidthProperty =
        AvaloniaProperty.Register<MasterDetailsView, double>(nameof(CompactWidth), 400);

    public static readonly StyledProperty<object> ItemsProperty =
        AvaloniaProperty.Register<MasterDetailsView, object>(nameof(Items));

    public static readonly StyledProperty<double> MasterPaneWidthProperty =
        AvaloniaProperty.Register<MasterDetailsView, double>(nameof(MasterPaneWidth), 200);

    public static readonly StyledProperty<object> HeaderProperty =
        AvaloniaProperty.Register<MasterDetailsView, object>(nameof(Header));

    public static readonly StyledProperty<object> FooterProperty =
        AvaloniaProperty.Register<MasterDetailsView, object>(nameof(Footer));

    public static readonly StyledProperty<IControlTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IControlTemplate?>(nameof(FooterTemplate));

    public static readonly StyledProperty<IControlTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<MasterDetailsView, IControlTemplate?>(nameof(HeaderTemplate));

    public static readonly StyledProperty<bool> IsCollapsedProperty =
        AvaloniaProperty.Register<MasterDetailsView, bool>(nameof(IsCollapsed));

    public static readonly StyledProperty<bool> AreDetailsShownProperty =
        AvaloniaProperty.Register<MasterDetailsView, bool>(nameof(AreDetailsShown));

    private ICommand backCommand = null!;

    private ICommand goToDetails = null!;

    public MasterDetailsView()
    {
        this.WhenAnyValue(x => x.SelectedItem)
            .WhereNotNull()
            .Do(_ => AreDetailsShown = true)
            .Subscribe();

        this.WhenAnyValue(x => x.Bounds, x => x.CompactWidth,
                (bounds, compactWidth) => bounds.Width > 0 && bounds.Width < compactWidth)
            .DistinctUntilChanged()
            .Do(collapsed => IsCollapsed = collapsed)
            .Subscribe();

        MessageBus.Current.SendMessage(new RegisterNavigation(this));

        BackCommand = ReactiveCommand.Create(() => AreDetailsShown = false);
        GoToDetails = ReactiveCommand.Create(() => AreDetailsShown = true);
    }

    public bool IsCollapsed
    {
        get => GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public IControlTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    public IControlTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object Footer
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

    [Content]
    public object Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? DetailsTemplate
    {
        get => GetValue(DetailsTemplateProperty);
        set => SetValue(DetailsTemplateProperty, value);
    }

    public bool IsBackButtonDisplayed
    {
        get => GetValue(IsBackButtonEnabledProperty);
        set => SetValue(IsBackButtonEnabledProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public bool AreDetailsShown
    {
        get => GetValue(AreDetailsShownProperty);
        set => SetValue(AreDetailsShownProperty, value);
    }

    public ICommand GoToDetails
    {
        get => goToDetails;
        private set => SetAndRaise(GoToDetailsProperty, ref goToDetails, value);
    }

    public ICommand BackCommand
    {
        get => backCommand;
        private set => SetAndRaise(BackCommandProperty, ref backCommand, value);
    }

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public void HideDetails()
    {
        AreDetailsShown = false;
    }
}

public class RegisterNavigation
{
    public RegisterNavigation(MasterDetailsView masterDetailsView)
    {
        MasterDetailsView = masterDetailsView;
    }

    public MasterDetailsView MasterDetailsView { get; }
}