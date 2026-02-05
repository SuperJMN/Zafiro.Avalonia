using System.Collections;
using Avalonia.Data.Converters;

namespace Zafiro.Avalonia.Converters;

public static class Enumerable
{
    public static FuncValueConverter<IEnumerable, bool> NotEmpty = new(enumerable =>
    {
        if (enumerable == null) return false;

        if (enumerable is ICollection collection)
            return collection.Count > 0;

        return enumerable.Cast<object>().Any();
    });

    public static FuncValueConverter<IEnumerable, bool> Empty = new(enumerable =>
    {
        if (enumerable == null) return true;

        if (enumerable is ICollection collection)
            return collection.Count == 0;

        return !enumerable.Cast<object>().Any();
    });

    public static FuncValueConverter<IEnumerable, int> Count = new(enumerable =>
    {
        if (enumerable == null) return 0;

        return enumerable.Cast<object>().Count();
    });
}