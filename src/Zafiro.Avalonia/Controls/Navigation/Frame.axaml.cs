using System.Windows.Input;
using Avalonia.Controls.Primitives;

namespace Zafiro.Avalonia.Controls.Navigation;

public class Frame : TemplatedControl
{
    public static readonly StyledProperty<ICommand> BackCommandProperty =
        AvaloniaProperty.Register<Frame, ICommand>(nameof(BackCommand));

    public static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<Frame, object>(nameof(Content));

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<Frame, object?>(nameof(Header));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<Frame, object?>(nameof(Footer));

    public ICommand BackCommand
    {
        get => GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }
}