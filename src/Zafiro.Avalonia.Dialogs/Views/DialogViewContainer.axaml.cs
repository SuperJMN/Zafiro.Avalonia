using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Zafiro.Avalonia.Dialogs.Views;

public class DialogViewContainer : ContentControl
{
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<DialogViewContainer, string>(
        nameof(Title));

    public static readonly StyledProperty<ICommand> CloseProperty = AvaloniaProperty.Register<DialogViewContainer, ICommand>(
        nameof(Close));

    public static readonly StyledProperty<double> ContentMaxWidthProperty = AvaloniaProperty.Register<DialogViewContainer, double>(
        nameof(ContentMaxWidth), double.PositiveInfinity);

    public static readonly StyledProperty<double> ContentMaxHeightProperty = AvaloniaProperty.Register<DialogViewContainer, double>(
        nameof(ContentMaxHeight), double.PositiveInfinity);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ICommand Close
    {
        get => GetValue(CloseProperty);
        set => SetValue(CloseProperty, value);
    }

    public double ContentMaxWidth
    {
        get => GetValue(ContentMaxWidthProperty);
        set => SetValue(ContentMaxWidthProperty, value);
    }

    public double ContentMaxHeight
    {
        get => GetValue(ContentMaxHeightProperty);
        set => SetValue(ContentMaxHeightProperty, value);
    }
}