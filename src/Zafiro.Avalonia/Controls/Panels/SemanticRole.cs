namespace Zafiro.Avalonia.Controls.Panels;

/// <summary>
/// Semantic role assigned to a child of <see cref="SemanticPanel"/>.
/// The panel uses this role to decide where to place the child depending on the available size.
/// </summary>
public enum SemanticRole
{
    /// <summary>Main content area — always visible and given the most space (e.g. video player, battle arena).</summary>
    Primary,

    /// <summary>Complementary content shown beside Primary on wide layouts, below on narrow (e.g. recommendations, team roster).</summary>
    Secondary,

    /// <summary>Contextual information placed below Primary (e.g. description, battle commentary).</summary>
    Info,

    /// <summary>Critical actions — always accessible, placed near thumb zone on mobile (e.g. play/pause, attack buttons).</summary>
    ActionPrimary,

    /// <summary>Less important actions that can be pushed further down or overflow (e.g. settings, share).</summary>
    ActionSecondary,

    /// <summary>Navigation or tool panel — side column on desktop, bottom bar on mobile (e.g. channel list, menu).</summary>
    Sidebar,
}