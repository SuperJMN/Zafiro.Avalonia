namespace Zafiro.Avalonia.Dialogs;

public static class DialogSizeCalculator
{
    public static (double MinWidth, double MaxWidth, double MaxHeight) Calculate(DialogSize size, double availableWidth, double availableHeight)
    {
        var resolved = size == DialogSize.Auto ? DialogSize.Standard : size;

        var (widthFraction, fallbackMaxWidth, minWidth) = resolved switch
        {
            DialogSize.Compact => (0.35, 360.0, 280.0),
            DialogSize.Standard => (0.50, 500.0, 320.0),
            DialogSize.Wide => (0.70, 720.0, 400.0),
            DialogSize.Full => (0.90, 960.0, 400.0),
            _ => (0.50, 500.0, 320.0)
        };

        var maxWidth = Math.Min(fallbackMaxWidth, availableWidth * widthFraction);
        maxWidth = Math.Max(maxWidth, minWidth);

        var maxHeight = availableHeight * 0.85;

        return (minWidth, maxWidth, maxHeight);
    }

    public static DialogSize Resolve(DialogSize declared)
    {
        return declared == DialogSize.Auto ? DialogSize.Standard : declared;
    }
}
