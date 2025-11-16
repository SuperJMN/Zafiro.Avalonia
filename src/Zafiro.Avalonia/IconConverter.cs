namespace Zafiro.Avalonia;

public class IconConverter : IIconConverter
{
    public static IconConverter Instance { get; } = new();

    public Control? Convert(IIcon icon)
    {
        var source = icon.Source;
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        var parts = source.Split(new[] { ':' }, 2);

        if (parts.Length == 2)
        {
            var prefix = parts[0];
            var valueWithoutPrefix = parts[1];

            if (Icons.IconControlProviderRegistry.TryGet(prefix, out var provider))
            {
                return provider.Create(icon, valueWithoutPrefix);
            }
        }

        var defaultProvider = Icons.IconControlProviderRegistry.DefaultProvider;
        if (defaultProvider != null)
        {
            return defaultProvider.Create(icon, source);
        }

        return null;
    }
}
