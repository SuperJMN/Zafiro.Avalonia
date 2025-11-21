namespace Zafiro.UI.Shell.Utils;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SectionGroupAttribute(string key, string friendlyName) : Attribute
{
    public string Key { get; } = key;

    public string FriendlyName { get; } = friendlyName;
}
