namespace Zafiro.Avalonia.Icons;

/// <summary>
/// Global registry for <see cref="IIconControlProvider"/> instances.
/// Callers can register providers identified by a prefix and optionally mark one as the default provider.
/// </summary>
public static class IconControlProviderRegistry
{
    static readonly Dictionary<string, IIconControlProvider> providers = new(StringComparer.OrdinalIgnoreCase);

    public static IIconControlProvider? DefaultProvider { get; private set; }

    public static IconControlProviderRegistryHandle Register(IIconControlProvider provider, bool asDefault = false)
    {
        if (provider is null) throw new ArgumentNullException(nameof(provider));

        providers[provider.Prefix] = provider;

        if (asDefault)
        {
            DefaultProvider = provider;
        }

        return new IconControlProviderRegistryHandle(provider);
    }

    public static bool TryGet(string prefix, out IIconControlProvider provider)
    {
        return providers.TryGetValue(prefix, out provider!);
    }

    public readonly struct IconControlProviderRegistryHandle
    {
        internal IconControlProviderRegistryHandle(IIconControlProvider provider)
        {
            Provider = provider;
        }

        public IIconControlProvider Provider { get; }
    }
}