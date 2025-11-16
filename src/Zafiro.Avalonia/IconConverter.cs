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
        string errorMessage;

        if (parts.Length == 2)
        {
            var prefix = parts[0];
            var valueWithoutPrefix = parts[1];

            if (Icons.IconControlProviderRegistry.TryGet(prefix, out var provider))
            {
                return provider.Create(icon, valueWithoutPrefix);
            }

            errorMessage = $"[Icon provider '{prefix}' is not registered for '{source}']";
        }
        else
        {
            errorMessage = $"[No icon provider found for '{source}' and no default provider is configured]";
        }

        var defaultProvider = Icons.IconControlProviderRegistry.DefaultProvider;
        if (defaultProvider != null)
        {
            return defaultProvider.Create(icon, source);
        }

        // Surface a descriptive error in the UI instead of failing silently
        // so consumers can quickly identify misconfigured or missing icon providers.
        return new TextBlock
        {
            Text = errorMessage
        };
    }
}
