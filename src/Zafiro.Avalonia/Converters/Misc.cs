using Avalonia.Data.Converters;

namespace Zafiro.Avalonia.Converters;

public class MiscConverters
{
    public static FuncValueConverter<SelectionKind, bool?> SelectionKindToCheckBoxValue = new FuncValueConverter<SelectionKind, bool?>(kind =>
    {
        return kind switch
        {
            SelectionKind.Full => true,
            SelectionKind.Partial => null,
            SelectionKind.None => false,
        };
    });


    public static FuncValueConverter<int, string> MoreThan99 = new FuncValueConverter<int, string>(i =>
    {
        return i switch
        {
            > 99 => "+99",
            _ => i.ToString()
        };
    });

    public static FuncMultiValueConverter<double, double> RangeToDegrees { get; } = new(doubles =>
    {
        var list = doubles.ToList();

        if (list is [var minimum, var maximum, var value])
        {
            return (value - minimum) / (maximum - minimum) * 360;
        }

        return 0;
    });

    public static FuncMultiValueConverter<double, double> Minimum { get; } = new(doubles =>
    {
        var list = doubles.ToList();

        if (list is [var a, var b])
        {
            return Math.Min(a, b);
        }

        return 0;
    });

    public static FuncValueConverter<int, IEnumerable<int>> Range { get; } = new(i => System.Linq.Enumerable.Range(1, i));
    public static FuncValueConverter<int, int> AddOne { get; } = new(i => i + 1);
    public static FuncValueConverter<bool, int> TrueToOne { get; } = new(i => i ? 1 : 0);
}