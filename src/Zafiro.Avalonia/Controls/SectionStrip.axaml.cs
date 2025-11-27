using System.Reactive.Disposables;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;
using DynamicData;
using Zafiro.Reactive;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls;

public class SectionStrip : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable<INavigationRoot>?> SectionsProperty = AvaloniaProperty.Register<SectionStrip, IEnumerable<INavigationRoot>?>(
        nameof(Sections));

    public static readonly StyledProperty<INavigationRoot> SelectedSectionProperty = AvaloniaProperty.Register<SectionStrip, INavigationRoot>(nameof(SelectedSection), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<SectionStrip, Orientation>(
        nameof(Orientation), Orientation.Vertical);

    public static readonly StyledProperty<double> MaxItemWidthProperty = AvaloniaProperty.Register<SectionStrip, double>(
        nameof(MaxItemWidth));

    public static readonly StyledProperty<double> MinItemWidthProperty = AvaloniaProperty.Register<SectionStrip, double>(
        nameof(MinItemWidth));

    public static readonly StyledProperty<double> ItemSpacingProperty = AvaloniaProperty.Register<SectionStrip, double>(
        nameof(ItemSpacing));

    public static readonly StyledProperty<double> IconSizeProperty = AvaloniaProperty.Register<SectionStrip, double>(
        nameof(IconSize), defaultValue: 38d);

    public static readonly StyledProperty<double> HorizontalIconLabelSpacingProperty = AvaloniaProperty.Register<SectionStrip, double>(
        nameof(HorizontalIconLabelSpacing));

    public static readonly StyledProperty<Thickness> ItemPaddingProperty = AvaloniaProperty.Register<SectionStrip, Thickness>(
        nameof(ItemPadding));

    public static readonly DirectProperty<SectionStrip, IEnumerable<INavigationRoot>> FilteredSectionsProperty = AvaloniaProperty.RegisterDirect<SectionStrip, IEnumerable<INavigationRoot>>(
        nameof(FilteredSections), o => o.FilteredSections, (o, v) => o.FilteredSections = v);

    public static readonly StyledProperty<Thickness> IconMarginProperty = AvaloniaProperty.Register<SectionStrip, Thickness>(
        nameof(IconMargin));

    public static readonly StyledProperty<double> VerticalIconLabelSpacingProperty = AvaloniaProperty.Register<SectionStrip, double>(
        nameof(VerticalIconLabelSpacing));

    public static readonly DirectProperty<SectionStrip, IEnumerable<SectionGroupView>> SectionGroupsProperty = AvaloniaProperty.RegisterDirect<SectionStrip, IEnumerable<SectionGroupView>>(
        nameof(SectionGroups), strip => strip.SectionGroups, (strip, value) => strip.SectionGroups = value);

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty = AvaloniaProperty.Register<SectionStrip, IDataTemplate?>(
        nameof(ItemTemplate));

    public static readonly StyledProperty<ControlTheme?> ItemContainerThemeProperty = AvaloniaProperty.Register<SectionStrip, ControlTheme?>(
        nameof(ItemContainerTheme));

    public static readonly StyledProperty<ControlTheme?> GroupHeaderThemeProperty = AvaloniaProperty.Register<SectionStrip, ControlTheme?>(
        nameof(GroupHeaderTheme));

    private readonly CompositeDisposable disposable = new();

    private IEnumerable<INavigationRoot> filteredSections = Enumerable.Empty<INavigationRoot>();

    private IEnumerable<SectionGroupView> sectionGroups = Enumerable.Empty<SectionGroupView>();

    public SectionStrip()
    {
        var sectionChanges = this.WhenAnyValue(strip => strip.Sections)
            .WhereNotNull()
            .Select(sections => sections.ToObservableChangeSetIfPossible(section => section.Name))
            .Switch();

        var sectionSorter = new SectionSorter(sectionChanges)
            .DisposeWith(disposable);

        FilteredSections = sectionSorter.Sections;

        var sectionGrouper = new SectionGrouper(sectionChanges)
            .DisposeWith(disposable);

        SectionGroups = sectionGrouper.SectionGroups;
    }

    public Thickness IconMargin
    {
        get => GetValue(IconMarginProperty);
        set => SetValue(IconMarginProperty, value);
    }

    public IEnumerable<INavigationRoot> FilteredSections
    {
        get => filteredSections;
        private set => SetAndRaise(FilteredSectionsProperty, ref filteredSections, value);
    }

    public IEnumerable<INavigationRoot>? Sections
    {
        get => GetValue(SectionsProperty);
        set => SetValue(SectionsProperty, value);
    }

    public INavigationRoot SelectedSection
    {
        get => GetValue(SelectedSectionProperty);
        set => SetValue(SelectedSectionProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double MaxItemWidth
    {
        get => GetValue(MaxItemWidthProperty);
        set => SetValue(MaxItemWidthProperty, value);
    }

    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public double HorizontalIconLabelSpacing
    {
        get => GetValue(HorizontalIconLabelSpacingProperty);
        set => SetValue(HorizontalIconLabelSpacingProperty, value);
    }

    public Thickness ItemPadding
    {
        get => GetValue(ItemPaddingProperty);
        set => SetValue(ItemPaddingProperty, value);
    }

    public double VerticalIconLabelSpacing
    {
        get => GetValue(VerticalIconLabelSpacingProperty);
        set => SetValue(VerticalIconLabelSpacingProperty, value);
    }

    public IEnumerable<SectionGroupView> SectionGroups
    {
        get => sectionGroups;
        private set => SetAndRaise(SectionGroupsProperty, ref sectionGroups, value);
    }

    /// <summary>
    /// Gets or sets the template used to display the content of each section item.
    /// When null, the default template (VerticalItem or HorizontalItem) is used based on orientation.
    /// </summary>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the theme applied to each <see cref="SectionStripItem"/> container.
    /// When null, the default SectionStripItem theme is used.
    /// </summary>
    public ControlTheme? ItemContainerTheme
    {
        get => GetValue(ItemContainerThemeProperty);
        set => SetValue(ItemContainerThemeProperty, value);
    }

    /// <summary>
    /// Gets or sets the theme applied to each <see cref="SectionStripGroupHeader"/>.
    /// When null, the default SectionStripGroupHeader theme is used.
    /// </summary>
    public ControlTheme? GroupHeaderTheme
    {
        get => GetValue(GroupHeaderThemeProperty);
        set => SetValue(GroupHeaderThemeProperty, value);
    }
}