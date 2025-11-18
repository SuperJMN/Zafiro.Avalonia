using Avalonia.Controls.Templates;
using Avalonia.Data;
using Zafiro.Avalonia.Controls.PropertyGrid.ViewModels;

namespace Zafiro.Avalonia.Controls.PropertyGrid;

public class PropertyEditorSelector : IDataTemplate
{
    public Dictionary<string, IDataTemplate> Editors { get; } = new();

    public Control? Build(object? param)
    {
        if (param is IPropertyItem pi)
        {
            var key = pi.PropertyType.FullName;
            if (key != null && Editors.TryGetValue(key, out var dt))
            {
                return dt.Build(param);
            }

            if (pi.PropertyType.IsEnum)
            {
                return new ComboBox
                {
                    ItemsSource = Enum.GetValues(pi.PropertyType),
                    [!ComboBox.SelectedItemProperty] = new Binding("Value")
                };
            }

            // Fallback for generic object or unregistered types
            return new TextBox
            {
                [!TextBox.TextProperty] = new Binding("Value")
            };
        }

        return null;
    }

    public bool Match(object? data)
    {
        return data is IPropertyItem;
    }
}