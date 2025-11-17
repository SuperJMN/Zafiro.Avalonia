using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.Models;

namespace Zafiro.Avalonia.Icons
{
    /// <summary>
    /// Icon provider that retrieves path strings from application resources
    /// and converts them into icon models for Projektanker.Icons.Avalonia.
    /// </summary>
    public class PathStringIconProvider : IIconProvider
    {
        // Default ViewBox for icons
        private static readonly ViewBoxModel DefaultViewBox = new(0, 0, 24, 24);
        private readonly Dictionary<string, IconModel> cache = new();
        private readonly string prefix;

        public PathStringIconProvider() : this("path")
        {
        }

        public PathStringIconProvider(string prefix)
        {
            this.prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        }

        public string Prefix => prefix;

        public IconModel GetIcon(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.StartsWith(prefix))
            {
                throw new ArgumentException($"The value '{value}' is not valid for this provider", nameof(value));
            }

            string iconName = value.Substring(prefix.Length + 1);

            if (cache.TryGetValue(iconName, out var icon))
            {
                return icon;
            }

            icon = CreateIconFromPathString(iconName);
            return cache[iconName] = icon;
        }

        private IconModel CreateIconFromPathString(string iconName)
        {
            // Look for the path string in resources (searching the full resource hierarchy)
            if (Application.Current is not IResourceHost host ||
                !ResourceNodeExtensions.TryFindResource(host, iconName, out var resource))
            {
                throw new KeyNotFoundException($"Resource '{iconName}' not found in application resources");
            }

            // The resource must be a string with path data
            if (resource is not string pathData)
            {
                throw new InvalidOperationException($"Resource '{iconName}' is not a path data string");
            }

            // 1. First, try to find a custom ViewBox for this icon
            var viewBox = TryGetCustomViewBox(iconName);

            // 2. If no custom ViewBox is found, calculate it from the path
            if (viewBox == null)
            {
                viewBox = CalculateViewBoxFromPath(pathData);
            }

            return new IconModel(
                viewBox,
                new PathModel(pathData));
        }

        private ViewBoxModel? TryGetCustomViewBox(string iconName)
        {
            // Try to find a custom ViewBox in resources (searching the full resource hierarchy)
            if (Application.Current is IResourceHost host &&
                ResourceNodeExtensions.TryFindResource(host, $"{iconName}_viewbox", out var viewBoxResource) &&
                viewBoxResource is string viewBoxString)
            {
                try
                {
                    return ViewBoxModel.Parse(viewBoxString);
                }
                catch
                {
                    // If there's an error parsing, fall back to the default value
                }
            }

            return null;
        }

        private ViewBoxModel CalculateViewBoxFromPath(string pathData)
        {
            try
            {
                // Parse the path to get a StreamGeometry
                var geometry = StreamGeometry.Parse(pathData);

                // Calculate the bounds of the geometry
                if (geometry.Bounds is var bounds && bounds != default)
                {
                    // Round to integers for the ViewBox
                    return new ViewBoxModel(
                        (int)Math.Floor(bounds.X),
                        (int)Math.Floor(bounds.Y),
                        (int)Math.Ceiling(bounds.Width),
                        (int)Math.Ceiling(bounds.Height));
                }
            }
            catch (Exception)
            {
                // In case of an error during parsing or calculation, use default value
            }

            // If calculation fails, use the default value
            return DefaultViewBox;
        }
    }
}