using System.Globalization;
using Avalonia.Data.Converters;
using Zafiro.Avalonia.Controls;

namespace Zafiro.Avalonia.Dialogs;

public class OptionRoleToButtonRoleConverter : IValueConverter
{
    public static OptionRoleToButtonRoleConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is OptionRole role)
        {
            return role switch
            {
                OptionRole.Primary => ButtonRole.Primary,
                OptionRole.Secondary => ButtonRole.Secondary,
                OptionRole.Destructive => ButtonRole.Destructive,
                OptionRole.Cancel => ButtonRole.Ghost,
                OptionRole.Info => ButtonRole.Link,
                _ => ButtonRole.Secondary
            };
        }

        return ButtonRole.Secondary;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}