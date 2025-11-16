namespace Zafiro.Avalonia.Icons;

/// <summary>
/// Provider that renders icons with the "svg" prefix, using Avalonia.Svg.Skia.
/// Examples:
///   svg:/Assets/Icons/sample.svg
///   svg:MyAssembly/Assets/Icons/sample.svg
/// </summary>
public class SvgIconControlProvider : IIconControlProvider
{
    public string Prefix => "svg";

    public Control? Create(Zafiro.UI.IIcon icon, string valueWithoutPrefix)
    {
        if (string.IsNullOrWhiteSpace(valueWithoutPrefix))
        {
            return null;
        }

        var remainder = valueWithoutPrefix;
        string assemblyName;
        string resourcePath;

        if (remainder.StartsWith("/", StringComparison.Ordinal))
        {
            assemblyName = Application.Current!.GetType().Assembly.GetName().Name!;
            resourcePath = remainder.TrimStart('/');
        }
        else
        {
            var idx = remainder.IndexOf('/');
            if (idx <= 0)
            {
                return null;
            }

            assemblyName = remainder[..idx];
            resourcePath = remainder[(idx + 1)..];
        }

        var uri = new Uri($"avares://{assemblyName}");
        return new global::Avalonia.Svg.Skia.Svg(uri) { Path = resourcePath };
    }
}
