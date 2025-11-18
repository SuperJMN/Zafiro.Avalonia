using Zafiro.Avalonia.Controls.PropertyGrid.ViewModels;

namespace Zafiro.Avalonia.Controls.PropertyGrid;

public partial class PropertyGrid : UserControl
{
    public static readonly StyledProperty<IList<object>> SelectedObjectsProperty =
        AvaloniaProperty.Register<PropertyGrid, IList<object>>(nameof(SelectedObjects), new List<object>());

    public PropertyGrid()
    {
        InitializeComponent();
        var selectedObjects = this.GetObservable(SelectedObjectsProperty);
        Items.DataContext = new PropertyGridViewModel(selectedObjects);
    }

    public IList<object> SelectedObjects
    {
        get => GetValue(SelectedObjectsProperty);
        set => SetValue(SelectedObjectsProperty, value);
    }
}