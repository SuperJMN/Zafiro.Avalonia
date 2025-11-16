namespace Zafiro.Avalonia.Icons;

/// <summary>
/// Abstraction that turns an <see cref="Zafiro.UI.IIcon"/> into an Avalonia <see cref="Control"/>,
/// identified by a textual prefix in the icon source.
/// </summary>
public interface IIconControlProvider
{
    /// <summary>
    /// Prefix used in <c>IIcon.Source</c> (e.g. "svg", "mdi", "fa").
    /// The prefix is compared case-insensitively.
    /// </summary>
    string Prefix { get; }

    /// <summary>
    /// Creates a control for the given icon.
    /// </summary>
    /// <param name="icon">The icon metadata.</param>
    /// <param name="valueWithoutPrefix">The source string without the "prefix:" part.</param>
    /// <returns>The control that visually represents the icon, or <c>null</c> if it cannot be created.</returns>
    Control? Create(Zafiro.UI.IIcon icon, string valueWithoutPrefix);
}
