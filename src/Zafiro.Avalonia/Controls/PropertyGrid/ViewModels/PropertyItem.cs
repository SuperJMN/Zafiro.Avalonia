using System.Reactive.Disposables;

namespace Zafiro.Avalonia.Controls.PropertyGrid.ViewModels;

public interface IPropertyItem
{
    string Name { get; }
    Type PropertyType { get; }
    object? Value { get; set; }
}

public class PropertyItem : ReactiveObject, IPropertyItem, IDisposable
{
    private readonly CompositeDisposable disposables = new();
    private readonly IList<object> targets;
    private object? value;

    public PropertyItem(string name, Type propertyType, IList<object> targets)
    {
        Name = name;
        PropertyType = propertyType;
        this.targets = targets;

        UpdateValueFromTargets();
    }

    public void Dispose()
    {
        disposables.Dispose();
    }

    public string Name { get; }
    public Type PropertyType { get; }

    public object? Value
    {
        get => value;
        set
        {
            this.RaiseAndSetIfChanged(ref this.value, value);
            SetTargetsValue(value);
        }
    }

    private void UpdateValueFromTargets()
    {
        if (!targets.Any()) return;

        var first = targets.First();
        var prop = first.GetType().GetProperty(Name);
        var val = prop?.GetValue(first);

        var allMatch = targets.All(t =>
        {
            var p = t.GetType().GetProperty(Name);
            var v = p?.GetValue(t);
            return Equals(v, val);
        });

        if (allMatch)
        {
            value = val;
            this.RaisePropertyChanged(nameof(Value));
        }
        else
        {
            value = GetDefault(PropertyType); // Or null?
            this.RaisePropertyChanged(nameof(Value));
        }
    }

    private static object? GetDefault(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }

    private void SetTargetsValue(object? newValue)
    {
        foreach (var target in targets)
        {
            var prop = target.GetType().GetProperty(Name);
            if (prop != null && prop.CanWrite)
            {
                try
                {
                    // Handle simple conversions if necessary, but usually binding does it.
                    // Especially important for Enum or numeric types.
                    if (newValue != null && !PropertyType.IsInstanceOfType(newValue))
                    {
                        var converted = Convert.ChangeType(newValue, PropertyType);
                        prop.SetValue(target, converted);
                    }
                    else
                    {
                        prop.SetValue(target, newValue);
                    }
                }
                catch (Exception)
                {
                    // Ignore setting errors for now
                }
            }
        }
    }
}