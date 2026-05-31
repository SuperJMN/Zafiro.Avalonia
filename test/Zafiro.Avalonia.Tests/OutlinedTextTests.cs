using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Zafiro.Avalonia.Controls;

namespace Zafiro.Avalonia.Tests;

public class OutlinedTextTests
{
    [AvaloniaFact]
    public void Stroke_thickness_increases_desired_size()
    {
        var plain = CreateOutlinedText("Outlined", strokeThickness: 0);
        var outlined = CreateOutlinedText("Outlined", strokeThickness: 8);

        plain.Measure(Size.Infinity);
        outlined.Measure(Size.Infinity);

        Assert.True(outlined.DesiredSize.Width > plain.DesiredSize.Width);
        Assert.True(outlined.DesiredSize.Height > plain.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Wrapped_text_uses_available_width()
    {
        var outlined = CreateOutlinedText("alpha beta gamma delta epsilon", strokeThickness: 4);
        var unwrapped = CreateOutlinedText("alpha beta gamma delta epsilon", strokeThickness: 4);
        unwrapped.TextWrapping = TextWrapping.NoWrap;

        unwrapped.Measure(Size.Infinity);
        outlined.Measure(new Size(120, double.PositiveInfinity));

        Assert.True(outlined.DesiredSize.Width <= 120);
        Assert.True(outlined.DesiredSize.Height > unwrapped.DesiredSize.Height);
    }

    private static OutlinedText CreateOutlinedText(string text, double strokeThickness)
    {
        return new OutlinedText
        {
            Text = text,
            FontSize = 24,
            Foreground = Brushes.White,
            Stroke = Brushes.Black,
            StrokeThickness = strokeThickness,
            TextWrapping = TextWrapping.Wrap,
        };
    }
}
