using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;

namespace Zafiro.Avalonia.Controls.SlimDataGrid;

public class Column : AvaloniaObject
{
    public static readonly StyledProperty<IDataTemplate?> TemplateProperty = AvaloniaProperty.Register<Column, IDataTemplate?>(
        nameof(CellTemplate));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty = AvaloniaProperty.Register<Column, IDataTemplate?>(
        nameof(HeaderTemplate));

    public static readonly StyledProperty<GridLength> WidthProperty = AvaloniaProperty.Register<Column, GridLength>(
        nameof(Width), GridLength.Star);

    public GridLength Width
    {
        get => GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    [Content]
    [InheritDataTypeFromItems(nameof(SlimDataGrid.ItemsSource), AncestorType = typeof(SlimDataGrid))]
    public IDataTemplate? CellTemplate
    {
        get => GetValue(TemplateProperty);
        set => SetValue(TemplateProperty, value);
    }

    [AssignBinding]
    [InheritDataTypeFromItems(nameof(SlimDataGrid.ItemsSource), AncestorType = typeof(SlimDataGrid))]
    public BindingBase? Binding { get; set; }

    public object? Header { get; set; }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }
}