using Avalonia.Media;

namespace Zafiro.Avalonia.Misc;

public static class ColorInterpolator
{
    public static Color InterpolateColor(List<Color> colors, double x)
    {
        if (colors == null || colors.Count == 0)
            throw new ArgumentException("The color list cannot be null or empty.");

        // Clamp x to a range between 0 and 1
        x = Math.Max(0, Math.Min(1, x));

        // Calculate the exact position inside the color list
        double scaledPosition = x * (colors.Count - 1);
        int index = (int)scaledPosition;
        double localX = scaledPosition - index;

        // If it is exactly on a color, return it directly
        if (index >= colors.Count - 1)
            return colors[^1];

        // Obtain the two colors to interpolate between
        Color colorA = colors[index];
        Color colorB = colors[index + 1];

        // Interpolate between the RGBA values of colorA and colorB
        byte a = (byte)(colorA.A + (colorB.A - colorA.A) * localX);
        byte r = (byte)(colorA.R + (colorB.R - colorA.R) * localX);
        byte g = (byte)(colorA.G + (colorB.G - colorA.G) * localX);
        byte b = (byte)(colorA.B + (colorB.B - colorA.B) * localX);

        return Color.FromArgb(a, r, g, b);
    }
}