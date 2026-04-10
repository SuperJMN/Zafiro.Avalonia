using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Avalonia.Markup.Xaml;
using JetBrains.Annotations;

namespace Zafiro.Avalonia.DesignTime;

[PublicAPI]
public class ReturnExtension : MarkupExtension
{
    public ReturnExtension()
    {
    }

    public ReturnExtension(object value)
    {
        Value = value;
    }

#pragma warning disable CS8764
    public override object? ProvideValue(IServiceProvider serviceProvider)
#pragma warning restore CS8764
    {
        if (Value is null)
        {
            return AvaloniaProperty.UnsetValue;
        }

        var service = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget))!;

        Type propertyType = service.TargetProperty switch
        {
            AvaloniaProperty ap => ap.PropertyType,
            PropertyInfo pi => pi.PropertyType,
            _ => throw new InvalidOperationException(
                $"Cannot determine property type from {service.TargetProperty?.GetType()}")
        };

        var type = propertyType.GenericTypeArguments[0];
        Type observableType = typeof(System.Reactive.Linq.Observable);
        MethodInfo returnMethodInfo = observableType.GetMethods().First(x => x.Name == "Return" && x.GetParameters().Length == 1);
        var method = returnMethodInfo.MakeGenericMethod(type);

        var finalValue = ProvideValue(type);

        var provideValue = method.Invoke(null, new[] { finalValue });
        return provideValue;
    }

    private object? ProvideValue(Type targetType)
    {
        object? finalValue;
        if (targetType.IsInstanceOfType(Value))
        {
            finalValue = Value;
        }
        else
        {
            var tc = TypeDescriptor.GetConverter(targetType);
            finalValue = tc.ConvertFrom(null, CultureInfo.InvariantCulture, Value!);
        }

        return finalValue;
    }

    public object? Value { get; set; }
}