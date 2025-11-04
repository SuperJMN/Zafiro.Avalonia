using System.Reactive.Disposables;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using DynamicData;
using ReactiveUI;
using System.Reactive;
using Zafiro.Reactive;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls;

public class SectionStrip : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable<ISection>?> SectionsProperty = AvaloniaProperty.Register<SectionStrip, IEnumerable<ISection>?>(
        nameof(Sections));

    public static readonly StyledProperty<ISection> SelectedSectionProperty = AvaloniaProperty.Register<SectionStrip, ISection>(nameof(SelectedSection), defaultBindingMode: BindingMode.TwoWay);

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

    public static readonly DirectProperty<SectionStrip, IEnumerable<ISection>> FilteredSectionsProperty = AvaloniaProperty.RegisterDirect<SectionStrip, IEnumerable<ISection>>(
        nameof(FilteredSections), o => o.FilteredSections, (o, v) => o.FilteredSections = v);

    public static readonly DirectProperty<SectionStrip, IEnumerable<ISection>> GroupedItemsProperty = AvaloniaProperty.RegisterDirect<SectionStrip, IEnumerable<ISection>>(
        nameof(GroupedItems), o => o.GroupedItems, (o, v) => o.GroupedItems = v);

    public static readonly StyledProperty<Thickness> IconMarginProperty = AvaloniaProperty.Register<SectionStrip, Thickness>(
        nameof(IconMargin));

    public static readonly StyledProperty<double> VerticalIconLabelSpacingProperty = AvaloniaProperty.Register<SectionStrip, double>(
        nameof(VerticalIconLabelSpacing));

    private readonly CompositeDisposable disposable = new();

    private IEnumerable<ISection> filteredSections;
    private IEnumerable<ISection> groupedItems = Array.Empty<ISection>();

    public SectionStrip()
    {
        var sectionChanges = this.WhenAnyValue(strip => strip.Sections)
            .WhereNotNull()
            .Select(sections => sections.OfType<INamedSection>().ToObservableChangeSetIfPossible(section => section.Name))
            .Switch();

        var sectionSorter = new SectionSorter(sectionChanges)
            .DisposeWith(disposable);

        FilteredSections = sectionSorter.Sections;

        // Rebuild grouped items whenever Sections changes or any section changes visibility/sort
        this.WhenAnyValue(x => x.Sections)
            .WhereNotNull()
            .Select(list =>
                list.OfType<ReactiveObject>()
                    .Select(ro => ro.Changed.Select(_ => Unit.Default))
                    .Merge()
                    .StartWith(Unit.Default))
            .Switch()
            .Subscribe(_ => RebuildGroupedItems())
            .DisposeWith(disposable);
    }

    private void RebuildGroupedItems()
    {
        var source = Sections?.ToList() ?? new List<ISection>();
        var output = new List<ISection>();
        var buffer = new List<ISection>();
        ISectionGroupHeader? currentHeader = null;

        void Flush()
        {
            var ordered = buffer.Where(s => s.IsVisible).OrderBy(s => s.SortOrder).ToList();
            output.AddRange(ordered);
            buffer.Clear();
        }

        foreach (var item in source)
        {
            if (item is ISectionGroupHeader header)
            {
                // Flush previous group
                Flush();
                // Add header if visible
                if (header.IsVisible)
                {
                    output.Add(header);
                }
                currentHeader = header;
                continue;
            }

            // Collect items under current group (or default group)
            buffer.Add(item);
        }

        Flush();
        GroupedItems = output;
    }

    public Thickness IconMargin
    {
        get => GetValue(IconMarginProperty);
        set => SetValue(IconMarginProperty, value);
    }

public IEnumerable<ISection> FilteredSections
{
    get => filteredSections;
    private set => SetAndRaise(FilteredSectionsProperty, ref filteredSections, value);
}

public IEnumerable<ISection> GroupedItems
{
    get => groupedItems;
    private set => SetAndRaise(GroupedItemsProperty, ref groupedItems, value);
}

    public IEnumerable<ISection>? Sections
    {
        get => GetValue(SectionsProperty);
        set => SetValue(SectionsProperty, value);
    }

    public ISection SelectedSection
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
}