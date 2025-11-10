using System.Globalization;
using Avalonia.Data.Converters;

namespace Zafiro.Avalonia.Converters;

public sealed class BytesToHumanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        if (!TryToDouble(value, out var bytes))
        {
            return value?.ToString();
        }

        int decimals = 0;
        if (parameter is int i)
        {
            decimals = i;
        }
        else if (parameter is string s && int.TryParse(s, out var p))
        {
            decimals = p;
        }

        return FormatBytes(bytes, decimals, culture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();

    private static bool TryToDouble(object value, out double result)
    {
        switch (value)
        {
            case double d:
                result = d;
                return true;
            case float f:
                result = f;
                return true;
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case decimal m:
                result = (double)m;
                return true;
            case string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed):
                result = parsed;
                return true;
            default:
                result = 0;
                return false;
        }
    }

    private static string FormatBytes(double bytes, int decimals, CultureInfo culture)
    {
        if (bytes < 0) bytes = 0;
        string[] units = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
        int order = 0;
        while (bytes >= 1024 && order < units.Length - 1)
        {
            order++;
            bytes /= 1024;
        }

        string fmt = "F" + Math.Max(0, decimals);
        return string.Create(culture, $"{bytes.ToString(fmt, culture)} {units[order]}");
    }
}