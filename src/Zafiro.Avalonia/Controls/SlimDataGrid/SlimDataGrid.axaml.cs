using System.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using DynamicData;
using Zafiro.Reactive;

namespace Zafiro.Avalonia.Controls.SlimDataGrid;

public class SlimDataGrid : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<SlimDataGrid, IEnumerable?>(
        nameof(ItemsSource));

    public static readonly DirectProperty<SlimDataGrid, IEnumerable<Row>?> DataRowsProperty = AvaloniaProperty.RegisterDirect<SlimDataGrid, IEnumerable<Row>?>(
        nameof(DataRows), o => o.DataRows, (o, v) => o.DataRows = v);

    public static readonly DirectProperty<SlimDataGrid, ColumnDefinitions?> ColumnDefinitionsProperty = AvaloniaProperty.RegisterDirect<SlimDataGrid, ColumnDefinitions?>(
        nameof(ColumnDefinitions), o => o.ColumnDefinitions, (o, v) => o.ColumnDefinitions = v);

    public static readonly DirectProperty<SlimDataGrid, IEnumerable<Header>?> HeadersProperty = AvaloniaProperty.RegisterDirect<SlimDataGrid, IEnumerable<Header>?>(
        nameof(Headers), o => o.Headers, (o, v) => o.Headers = v);

    public static readonly StyledProperty<bool> ShowHeadersProperty = AvaloniaProperty.Register<SlimDataGrid, bool>(
        nameof(ShowHeaders), true);

    public static readonly StyledProperty<Thickness> HeaderBorderThicknessProperty = AvaloniaProperty.Register<SlimDataGrid, Thickness>(
        nameof(HeaderBorderThickness));

    public static readonly StyledProperty<IBrush?> HeaderBorderBrushProperty = AvaloniaProperty.Register<SlimDataGrid, IBrush?>(
        nameof(HeaderBorderBrush));

    public static readonly StyledProperty<IBrush?> HeaderBackgroundProperty = AvaloniaProperty.Register<SlimDataGrid, IBrush?>(
        nameof(HeaderBackground));

    public static readonly StyledProperty<ControlTheme?> RowThemeProperty = AvaloniaProperty.Register<SlimDataGrid, ControlTheme?>(
        nameof(RowTheme));

    public static readonly StyledProperty<Thickness> HeaderPaddingProperty = AvaloniaProperty.Register<SlimDataGrid, Thickness>(
        nameof(HeaderPadding));

    private ColumnDefinitions? columnDefinitions;

    private IEnumerable<Row>? dataRows;

    private IEnumerable<Header>? headers;

    public SlimDataGrid()
    {
        this.WhenAnyValue(x => x.Columns)
            .WhereNotNull()
            .Select(cols => cols.Select((col, index) => new Header(col, index)))
            .Subscribe(h => Headers = h);

        this.WhenAnyValue(x => x.ItemsSource, x => x.RowTheme, x => x.Columns, (items, rowTheme, columns) => (items, rowTheme, columns))
            .Where(x => x.columns != null && x.items != null)
            .Select(source => source.items.Cast<object>()
                .ToObservableChangeSetIfPossible()
                .Transform(item => new Row(item, Columns) { Theme = RowTheme }))
            .Switch()
            .Bind(out var rows)
            .Subscribe();

        DataRows = rows;
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IEnumerable<Row>? DataRows
    {
        get => dataRows;
        private set => SetAndRaise(DataRowsProperty, ref dataRows, value);
    }

    public ColumnDefinitions? ColumnDefinitions
    {
        get => columnDefinitions;
        private set => SetAndRaise(ColumnDefinitionsProperty, ref columnDefinitions, value);
    }

    public Columns Columns { get; } = new();

    public IEnumerable<Header>? Headers
    {
        get => headers;
        private set => SetAndRaise(HeadersProperty, ref headers, value);
    }

    public bool ShowHeaders
    {
        get => GetValue(ShowHeadersProperty);
        set => SetValue(ShowHeadersProperty, value);
    }

    public Thickness HeaderBorderThickness
    {
        get => GetValue(HeaderBorderThicknessProperty);
        set => SetValue(HeaderBorderThicknessProperty, value);
    }

    public IBrush? HeaderBorderBrush
    {
        get => GetValue(HeaderBorderBrushProperty);
        set => SetValue(HeaderBorderBrushProperty, value);
    }

    public IBrush? HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public ControlTheme? RowTheme
    {
        get => GetValue(RowThemeProperty);
        set => SetValue(RowThemeProperty, value);
    }

    public Thickness HeaderPadding
    {
        get => GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }
}
