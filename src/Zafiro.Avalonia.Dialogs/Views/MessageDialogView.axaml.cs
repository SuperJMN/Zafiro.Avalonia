using Avalonia;
using Avalonia.Controls;

namespace Zafiro.Avalonia.Dialogs.Views;

public partial class MessageDialogView : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<MessageDialogView, string?>(nameof(Text));

    public MessageDialogView()
    {
        InitializeComponent();
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
