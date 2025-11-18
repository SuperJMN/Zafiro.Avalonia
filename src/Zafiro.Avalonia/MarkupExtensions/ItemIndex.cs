using System.Collections;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace Zafiro.Avalonia.MarkupExtensions;

public class ItemIndex : MarkupExtension
{
    public IValueConverter? Converter { get; set; }
    public object? ConverterParameter { get; set; }
    public bool StartFromOne { get; set; }
    public string StringFormat { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new MultiBinding
        {
            StringFormat = this.StringFormat,
            Converter = new ContainerIdConverter()
            {
                Converter = this.Converter,
                ConverterParameter = this.ConverterParameter,
                StartFromOne = this.StartFromOne,
            },
            Bindings =
            [
                // 0: ItemsControl
                new Binding
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                    {
                        AncestorType = typeof(ItemsControl)
                    }
                },
                // 1: Item itself
                new Binding(),
                // 2: ItemsSource.Count that forces reevaluation
                new Binding
                {
                    Path = "ItemsSource.Count",
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                    {
                        AncestorType = typeof(ItemsControl)
                    }
                }
            ]
        };
    }
}

public class ContainerIdConverter : IMultiValueConverter
{
    public IValueConverter? Converter { get; set; }
    public object? ConverterParameter { get; set; }
    public bool StartFromOne { get; set; }

    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (values.Count < 2
            || values[0] is not ItemsControl itemsControl
            || values[1] is null)
        {
            return AvaloniaProperty.UnsetValue;
        }

        var source = itemsControl.ItemsSource as IList
                     ?? itemsControl.Items as IList;

        if (source == null)
            return AvaloniaProperty.UnsetValue;

        var index = source.IndexOf(values[1]!);
        if (index < 0)
            return AvaloniaProperty.UnsetValue;

        if (StartFromOne)
            index++;

        // 1) If there is a Converter, delegate to it
        if (Converter != null)
            return Converter.Convert(index, targetType, ConverterParameter, culture);

        // 2) If the target is string, return ToString
        if (targetType == typeof(string))
            return index.ToString(culture);

        // 3) For other types, rely on ChangeType
        return System.Convert.ChangeType(index, targetType, culture);
    }

    public object[] ConvertBack(
        object? value,
        Type[] targetTypes,
        object? parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}