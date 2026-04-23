using Avalonia.Layout;
using Avalonia.Media;
using Zafiro.Avalonia.Controls;

namespace Zafiro.Avalonia.Icons;

/// <summary>
/// Provider that renders any text/glyph (emoji, Unicode character, font ligature, monogram, …)
/// as a <see cref="TextBlock"/>. Use the <c>text:</c> prefix in <see cref="IIcon.Source"/>:
/// <code>{Icon text:😘}</code> or <code>{Icon text:★}</code>.
/// Honors <see cref="IconOptions.FillProperty"/> (mapped to Foreground) and
/// <see cref="IconOptions.SizeProperty"/> (mapped to FontSize).
/// </summary>
public class TextIconControlProvider : IIconControlProvider
{
    public string Prefix => "text";

    public Control? Create(IIcon icon, string valueWithoutPrefix)
    {
        if (string.IsNullOrEmpty(valueWithoutPrefix))
        {
            return null;
        }

        var textBlock = new TextBlock
        {
            Text = valueWithoutPrefix,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        textBlock[!TextBlock.ForegroundProperty] = textBlock[!IconOptions.FillProperty];
        textBlock[!TextBlock.FontSizeProperty] = textBlock[!IconOptions.SizeProperty];

        return textBlock;
    }
}
