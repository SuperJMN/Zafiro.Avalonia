using Avalonia.Data.Converters;

namespace Zafiro.Avalonia.Controls.Navigation;

public static class FrameConverters
{
    public static readonly FuncValueConverter<object?, object?> HeaderContext =
        new(content => content is Control control ? control.DataContext : content);
}