using System.ComponentModel;
using System.Globalization;

namespace Zafiro.Avalonia.Behaviors;

public static class EqualitySetterHelper
{
    public static void SetValue(
        StyledElement element,
        object bindingValue,
        object compareValue,
        AvaloniaProperty targetProperty,
        object trueValue,
        object falseValue)
    {
        bool isMatch = Equals(bindingValue, compareValue);
        object result = isMatch ? trueValue : falseValue;

        // Conversion when needed: for example, from string to bool.
        Type targetType = targetProperty.PropertyType;
        if (result != null && !targetType.IsInstanceOfType(result))
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null && converter.CanConvertFrom(result.GetType()))
                    result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, result);
                else
                    result = System.Convert.ChangeType(result, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while converting: " + ex.Message);
            }
        }

        element.SetValue(targetProperty, result);
    }
}