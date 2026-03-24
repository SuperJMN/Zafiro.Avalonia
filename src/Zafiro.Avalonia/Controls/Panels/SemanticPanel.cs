using Avalonia.Media;
using Avalonia.VisualTree;

namespace Zafiro.Avalonia.Controls.Panels;

/// <summary>
/// A panel that distributes children based on their <see cref="SemanticRole"/>,
/// automatically adapting the layout to the available width (Compact / Medium / Expanded).
/// </summary>
public class SemanticPanel : Panel
{
    private const double HysteresisMargin = 20;

    public static readonly AttachedProperty<SemanticRole?> RoleProperty =
        AvaloniaProperty.RegisterAttached<SemanticPanel, Control, SemanticRole?>("Role");

    public static readonly StyledProperty<double> CompactBreakpointProperty =
        AvaloniaProperty.Register<SemanticPanel, double>(nameof(CompactBreakpoint), 600);

    public static readonly StyledProperty<double> ExpandedBreakpointProperty =
        AvaloniaProperty.Register<SemanticPanel, double>(nameof(ExpandedBreakpoint), 900);

    public static readonly StyledProperty<double> PrimaryRatioProperty =
        AvaloniaProperty.Register<SemanticPanel, double>(nameof(PrimaryRatio), 0.65);

    public static readonly StyledProperty<double> SidebarWidthProperty =
        AvaloniaProperty.Register<SemanticPanel, double>(nameof(SidebarWidth), 250);

    public static readonly StyledProperty<double> SecondaryMinWidthProperty =
        AvaloniaProperty.Register<SemanticPanel, double>(nameof(SecondaryMinWidth), 200);

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<SemanticPanel, double>(nameof(Spacing), 8);

    public static readonly DirectProperty<SemanticPanel, SizeClass> CurrentSizeClassProperty =
        AvaloniaProperty.RegisterDirect<SemanticPanel, SizeClass>(nameof(CurrentSizeClass), o => o.CurrentSizeClass);

    private SizeClass currentSizeClass;

    static SemanticPanel()
    {
        AffectsMeasure<SemanticPanel>(
            CompactBreakpointProperty,
            ExpandedBreakpointProperty,
            PrimaryRatioProperty,
            SidebarWidthProperty,
            SecondaryMinWidthProperty,
            SpacingProperty);

        RoleProperty.Changed.AddClassHandler<Control>((c, _) =>
        {
            if (c.GetVisualParent() is SemanticPanel panel)
            {
                panel.InvalidateMeasure();
            }
        });
    }

    public double CompactBreakpoint
    {
        get => GetValue(CompactBreakpointProperty);
        set => SetValue(CompactBreakpointProperty, value);
    }

    public double ExpandedBreakpoint
    {
        get => GetValue(ExpandedBreakpointProperty);
        set => SetValue(ExpandedBreakpointProperty, value);
    }

    public double PrimaryRatio
    {
        get => GetValue(PrimaryRatioProperty);
        set => SetValue(PrimaryRatioProperty, value);
    }

    public double SidebarWidth
    {
        get => GetValue(SidebarWidthProperty);
        set => SetValue(SidebarWidthProperty, value);
    }

    public double SecondaryMinWidth
    {
        get => GetValue(SecondaryMinWidthProperty);
        set => SetValue(SecondaryMinWidthProperty, value);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public SizeClass CurrentSizeClass
    {
        get => currentSizeClass;
        private set => SetAndRaise(CurrentSizeClassProperty, ref currentSizeClass, value);
    }

    public static SemanticRole? GetRole(Control control) => control.GetValue(RoleProperty);
    public static void SetRole(Control control, SemanticRole? value) => control.SetValue(RoleProperty, value);

    protected override Size MeasureOverride(Size availableSize)
    {
        var slots = ResolveSlots(availableSize);
        foreach (var slot in slots)
        {
            slot.Control.Measure(new Size(slot.Width, slot.AvailableHeight));
        }

        return ComputeDesiredSize(slots, availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var slots = ResolveSlots(finalSize);

        foreach (var slot in slots)
        {
            slot.Control.Measure(new Size(slot.Width, slot.AvailableHeight));
        }

        var rects = ComputeArrangeRects(slots, finalSize);

        for (var i = 0; i < slots.Length; i++)
        {
            var rect = FlowDirection == FlowDirection.RightToLeft
                ? MirrorHorizontally(rects[i], finalSize.Width)
                : rects[i];

            slots[i].Control.Arrange(rect);
        }

        return finalSize;
    }

    private Slot[] ResolveSlots(Size availableSize)
    {
        var sizeClass = ResolveSizeClass(availableSize.Width);
        CurrentSizeClass = sizeClass;

        var children = ResolveChildren();

        return sizeClass switch
        {
            SizeClass.Expanded => BuildExpandedSlots(children, availableSize),
            SizeClass.Medium => BuildMediumSlots(children, availableSize),
            _ => BuildCompactSlots(children, availableSize),
        };
    }

    private SizeClass ResolveSizeClass(double width)
    {
        var compact = CompactBreakpoint;
        var expanded = ExpandedBreakpoint;

        return currentSizeClass switch
        {
            SizeClass.Compact when width >= compact + HysteresisMargin => width >= expanded + HysteresisMargin
                ? SizeClass.Expanded
                : SizeClass.Medium,
            SizeClass.Medium when width < compact - HysteresisMargin => SizeClass.Compact,
            SizeClass.Medium when width >= expanded + HysteresisMargin => SizeClass.Expanded,
            SizeClass.Expanded when width < expanded - HysteresisMargin => width < compact - HysteresisMargin
                ? SizeClass.Compact
                : SizeClass.Medium,
            _ when currentSizeClass == default && width >= expanded => SizeClass.Expanded,
            _ when currentSizeClass == default && width >= compact => SizeClass.Medium,
            _ when currentSizeClass == default => SizeClass.Compact,
            _ => currentSizeClass,
        };
    }

    private RoleMap ResolveChildren()
    {
        var map = new RoleMap();
        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }

            var role = GetRole(child);
            switch (role)
            {
                case SemanticRole.Primary when map.Primary is null:
                    map.Primary = child;
                    break;
                case SemanticRole.Secondary when map.Secondary is null:
                    map.Secondary = child;
                    break;
                case SemanticRole.Info when map.Info is null:
                    map.Info = child;
                    break;
                case SemanticRole.ActionPrimary when map.ActionPrimary is null:
                    map.ActionPrimary = child;
                    break;
                case SemanticRole.ActionSecondary when map.ActionSecondary is null:
                    map.ActionSecondary = child;
                    break;
                case SemanticRole.Sidebar when map.Sidebar is null:
                    map.Sidebar = child;
                    break;
            }
        }

        return map;
    }

    // ── Expanded layout ──────────────────────────────────────────────
    //  ┌────────┬──────────────────────┬───────────────┐
    //  │        │     Primary          │  Secondary    │
    //  │Sidebar │                      │               │
    //  │        ├──────────────────────┤               │
    //  │        │ Info                 │               │
    //  │        ├──────────────────────┴───────────────┤
    //  │        │ ActionPrimary        │ActionSecondary│
    //  └────────┴──────────────────────────────────────┘

    private Slot[] BuildExpandedSlots(RoleMap map, Size available)
    {
        var spacing = Spacing;
        var slots = new List<Slot>();

        var sidebarW = map.Sidebar is not null ? SidebarWidth : 0;
        var sidebarSpacing = map.Sidebar is not null ? spacing : 0;

        var contentWidth = available.Width - sidebarW - sidebarSpacing;

        var hasSecondary = map.Secondary is not null;
        var secondaryW = hasSecondary ? Math.Max(SecondaryMinWidth, contentWidth * (1 - PrimaryRatio)) : 0;
        var secondarySpacing = hasSecondary ? spacing : 0;

        if (hasSecondary && secondaryW > contentWidth * 0.5)
        {
            secondaryW = contentWidth * (1 - PrimaryRatio);
        }

        var primaryW = contentWidth - secondaryW - secondarySpacing;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }

            var role = GetRole(child);
            var width = role switch
            {
                SemanticRole.Sidebar => sidebarW,
                SemanticRole.Primary => primaryW,
                SemanticRole.Info => primaryW,
                SemanticRole.Secondary => secondaryW,
                SemanticRole.ActionPrimary => map.ActionSecondary is not null
                    ? contentWidth * PrimaryRatio
                    : contentWidth,
                SemanticRole.ActionSecondary => contentWidth * (1 - PrimaryRatio),
                _ => contentWidth,
            };

            if (ReferenceEquals(child, map.Primary) ||
                ReferenceEquals(child, map.Secondary) ||
                ReferenceEquals(child, map.Info) ||
                ReferenceEquals(child, map.ActionPrimary) ||
                ReferenceEquals(child, map.ActionSecondary) ||
                ReferenceEquals(child, map.Sidebar))
            {
                slots.Add(new Slot(child, Math.Max(0, width), available.Height));
            }
        }

        return slots.ToArray();
    }

    // ── Medium layout ────────────────────────────────────────────────
    //  ┌──────────────────────┬───────────────┐
    //  │     Primary          │  Secondary    │
    //  │                      │               │
    //  ├──────────────────────┤               │
    //  │ Info                 │               │
    //  ├──────────────────────┴───────────────┤
    //  │ ActionPrimary        │ActionSecondary│
    //  ├──────────────────────────────────────┤
    //  │ Sidebar (horizontal bar)             │
    //  └──────────────────────────────────────┘

    private Slot[] BuildMediumSlots(RoleMap map, Size available)
    {
        var spacing = Spacing;
        var totalWidth = available.Width;

        var hasSecondary = map.Secondary is not null;
        var secondarySideBySide = hasSecondary && totalWidth * (1 - PrimaryRatio) >= SecondaryMinWidth;

        var secondaryW = secondarySideBySide ? totalWidth * (1 - PrimaryRatio) : totalWidth;
        var secondarySpacing = secondarySideBySide ? spacing : 0;
        var primaryW = secondarySideBySide ? totalWidth - secondaryW - secondarySpacing : totalWidth;

        var slots = new List<Slot>();

        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }

            var role = GetRole(child);
            var width = role switch
            {
                SemanticRole.Primary => primaryW,
                SemanticRole.Secondary => secondaryW,
                SemanticRole.Info => secondarySideBySide ? primaryW : totalWidth,
                SemanticRole.ActionPrimary => map.ActionSecondary is not null ? totalWidth * PrimaryRatio : totalWidth,
                SemanticRole.ActionSecondary => totalWidth * (1 - PrimaryRatio),
                SemanticRole.Sidebar => totalWidth,
                _ => totalWidth,
            };

            if (ReferenceEquals(child, map.Primary) ||
                ReferenceEquals(child, map.Secondary) ||
                ReferenceEquals(child, map.Info) ||
                ReferenceEquals(child, map.ActionPrimary) ||
                ReferenceEquals(child, map.ActionSecondary) ||
                ReferenceEquals(child, map.Sidebar))
            {
                slots.Add(new Slot(child, Math.Max(0, width), available.Height));
            }
        }

        return slots.ToArray();
    }

    // ── Compact layout ───────────────────────────────────────────────
    //  ┌───────────────────┐
    //  │   Primary         │
    //  ├───────────────────┤
    //  │ ActionPrimary     │
    //  ├───────────────────┤
    //  │ Info              │
    //  ├───────────────────┤
    //  │ Secondary         │
    //  ├───────────────────┤
    //  │ ActionSecondary   │
    //  ├───────────────────┤
    //  │ Sidebar (bar)     │
    //  └───────────────────┘

    private Slot[] BuildCompactSlots(RoleMap map, Size available)
    {
        var totalWidth = available.Width;

        var orderedRoles = new[]
        {
            (map.Primary, SemanticRole.Primary),
            (map.ActionPrimary, SemanticRole.ActionPrimary),
            (map.Info, SemanticRole.Info),
            (map.Secondary, SemanticRole.Secondary),
            (map.ActionSecondary, SemanticRole.ActionSecondary),
            (map.Sidebar, SemanticRole.Sidebar),
        };

        return orderedRoles
            .Where(r => r.Item1 is not null)
            .Select(r => new Slot(r.Item1!, totalWidth, available.Height))
            .ToArray();
    }

    // ── Arrange geometry computation ─────────────────────────────────

    private Rect[] ComputeArrangeRects(Slot[] slots, Size finalSize)
    {
        return CurrentSizeClass switch
        {
            SizeClass.Expanded => ArrangeExpanded(slots, finalSize),
            SizeClass.Medium => ArrangeMedium(slots, finalSize),
            _ => ArrangeCompact(slots, finalSize),
        };
    }

    private Rect[] ArrangeExpanded(Slot[] slots, Size finalSize)
    {
        var spacing = Spacing;
        var map = ResolveChildren();

        var sidebarW = map.Sidebar is not null ? SidebarWidth : 0;
        var sidebarSpacing = map.Sidebar is not null ? spacing : 0;
        var contentLeft = sidebarW + sidebarSpacing;

        var contentWidth = finalSize.Width - contentLeft;
        var hasSecondary = map.Secondary is not null;
        var secondaryW = hasSecondary ? Math.Max(SecondaryMinWidth, contentWidth * (1 - PrimaryRatio)) : 0;

        if (hasSecondary && secondaryW > contentWidth * 0.5)
        {
            secondaryW = contentWidth * (1 - PrimaryRatio);
        }

        var secondarySpacing = hasSecondary ? spacing : 0;
        var primaryW = contentWidth - secondaryW - secondarySpacing;

        var primaryH = FindDesired(slots, map.Primary).Height;
        var infoH = FindDesired(slots, map.Info).Height;
        var actionPrimaryH = FindDesired(slots, map.ActionPrimary).Height;
        var actionSecondaryH = FindDesired(slots, map.ActionSecondary).Height;
        var actionsH = Math.Max(actionPrimaryH, actionSecondaryH);

        var leftColumnH = primaryH + (map.Info is not null ? spacing + infoH : 0);
        var secondaryH = FindDesired(slots, map.Secondary).Height;
        var rightColumnH = secondaryH;
        var mainRowsH = Math.Max(leftColumnH, rightColumnH);

        var actionsSpacing = actionsH > 0 ? spacing : 0;

        var rects = new Rect[slots.Length];

        for (var i = 0; i < slots.Length; i++)
        {
            var role = GetRole(slots[i].Control);
            rects[i] = role switch
            {
                SemanticRole.Sidebar => new Rect(0, 0, sidebarW, finalSize.Height),
                SemanticRole.Primary => new Rect(contentLeft, 0, primaryW, primaryH),
                SemanticRole.Info => new Rect(contentLeft, primaryH + spacing, primaryW, infoH),
                SemanticRole.Secondary => new Rect(contentLeft + primaryW + secondarySpacing, 0, secondaryW, mainRowsH),
                SemanticRole.ActionPrimary => new Rect(contentLeft, mainRowsH + actionsSpacing,
                    map.ActionSecondary is not null ? contentWidth * PrimaryRatio : contentWidth, actionsH),
                SemanticRole.ActionSecondary => new Rect(contentLeft + contentWidth * PrimaryRatio + spacing,
                    mainRowsH + actionsSpacing,
                    contentWidth * (1 - PrimaryRatio) - spacing, actionsH),
                _ => default,
            };
        }

        return rects;
    }

    private Rect[] ArrangeMedium(Slot[] slots, Size finalSize)
    {
        var spacing = Spacing;
        var totalWidth = finalSize.Width;
        var map = ResolveChildren();

        var hasSecondary = map.Secondary is not null;
        var sideBySide = hasSecondary && totalWidth * (1 - PrimaryRatio) >= SecondaryMinWidth;

        var secondaryW = sideBySide ? totalWidth * (1 - PrimaryRatio) : totalWidth;
        var secondarySpacing = sideBySide ? spacing : 0;
        var primaryW = sideBySide ? totalWidth - secondaryW - secondarySpacing : totalWidth;

        var primaryH = FindDesired(slots, map.Primary).Height;
        var infoH = FindDesired(slots, map.Info).Height;
        var secondaryH = FindDesired(slots, map.Secondary).Height;
        var actionPrimaryH = FindDesired(slots, map.ActionPrimary).Height;
        var actionSecondaryH = FindDesired(slots, map.ActionSecondary).Height;
        var actionsH = Math.Max(actionPrimaryH, actionSecondaryH);
        var sidebarH = FindDesired(slots, map.Sidebar).Height;

        var rects = new Rect[slots.Length];
        var y = 0d;

        if (sideBySide)
        {
            var leftH = primaryH + (map.Info is not null ? spacing + infoH : 0);
            var mainRowsH = Math.Max(leftH, secondaryH);

            for (var i = 0; i < slots.Length; i++)
            {
                var role = GetRole(slots[i].Control);
                rects[i] = role switch
                {
                    SemanticRole.Primary => new Rect(0, 0, primaryW, primaryH),
                    SemanticRole.Info => new Rect(0, primaryH + spacing, primaryW, infoH),
                    SemanticRole.Secondary => new Rect(primaryW + secondarySpacing, 0, secondaryW, mainRowsH),
                    SemanticRole.ActionPrimary => new Rect(0, mainRowsH + spacing,
                        map.ActionSecondary is not null ? totalWidth * PrimaryRatio : totalWidth, actionsH),
                    SemanticRole.ActionSecondary => new Rect(totalWidth * PrimaryRatio + spacing, mainRowsH + spacing,
                        totalWidth * (1 - PrimaryRatio) - spacing, actionsH),
                    SemanticRole.Sidebar => new Rect(0, mainRowsH + spacing + (actionsH > 0 ? actionsH + spacing : 0),
                        totalWidth, sidebarH),
                    _ => default,
                };
            }
        }
        else
        {
            // Stacked: Primary, Info, Secondary, Actions, Sidebar
            var actionY = 0d;
            for (var i = 0; i < slots.Length; i++)
            {
                var role = GetRole(slots[i].Control);
                rects[i] = role switch
                {
                    SemanticRole.Primary => Advance(ref y, 0, totalWidth, primaryH, spacing),
                    SemanticRole.Info => Advance(ref y, 0, totalWidth, infoH, spacing),
                    SemanticRole.Secondary => Advance(ref y, 0, totalWidth, secondaryH, spacing),
                    SemanticRole.ActionPrimary => Advance(ref y, 0,
                        map.ActionSecondary is not null ? totalWidth * PrimaryRatio : totalWidth, actionsH, spacing),
                    SemanticRole.ActionSecondary => new Rect(totalWidth * PrimaryRatio + spacing, actionY,
                        totalWidth * (1 - PrimaryRatio) - spacing, actionsH),
                    SemanticRole.Sidebar => Advance(ref y, 0, totalWidth, sidebarH, spacing),
                    _ => default,
                };

                if (role == SemanticRole.ActionPrimary)
                {
                    actionY = rects[i].Y;
                }
            }
        }

        return rects;
    }

    private Rect[] ArrangeCompact(Slot[] slots, Size finalSize)
    {
        var spacing = Spacing;
        var totalWidth = finalSize.Width;
        var rects = new Rect[slots.Length];
        var y = 0d;

        for (var i = 0; i < slots.Length; i++)
        {
            var h = slots[i].Control.DesiredSize.Height;
            rects[i] = Advance(ref y, 0, totalWidth, h, spacing);
        }

        return rects;
    }

    // ── Desired size computation ─────────────────────────────────────

    private Size ComputeDesiredSize(Slot[] slots, Size available)
    {
        return CurrentSizeClass switch
        {
            SizeClass.Expanded => DesiredExpanded(slots, available),
            SizeClass.Medium => DesiredMedium(slots, available),
            _ => DesiredCompact(slots, available),
        };
    }

    private Size DesiredExpanded(Slot[] slots, Size available)
    {
        var spacing = Spacing;
        var map = ResolveChildren();

        var sidebarW = map.Sidebar is not null ? SidebarWidth : 0;
        var sidebarSpacing = map.Sidebar is not null ? spacing : 0;

        var primaryH = FindDesired(slots, map.Primary).Height;
        var infoH = FindDesired(slots, map.Info).Height;
        var secondaryH = FindDesired(slots, map.Secondary).Height;
        var actionH = Math.Max(FindDesired(slots, map.ActionPrimary).Height,
            FindDesired(slots, map.ActionSecondary).Height);

        var leftH = primaryH + (map.Info is not null ? spacing + infoH : 0);
        var mainH = Math.Max(leftH, secondaryH);
        var totalH = mainH + (actionH > 0 ? spacing + actionH : 0);

        var sidebarTotalH = FindDesired(slots, map.Sidebar).Height;

        return new Size(available.Width, Math.Max(totalH, sidebarTotalH));
    }

    private Size DesiredMedium(Slot[] slots, Size available)
    {
        var spacing = Spacing;
        var map = ResolveChildren();
        var sideBySide = map.Secondary is not null && available.Width * (1 - PrimaryRatio) >= SecondaryMinWidth;

        var primaryH = FindDesired(slots, map.Primary).Height;
        var infoH = FindDesired(slots, map.Info).Height;
        var secondaryH = FindDesired(slots, map.Secondary).Height;
        var actionH = Math.Max(FindDesired(slots, map.ActionPrimary).Height,
            FindDesired(slots, map.ActionSecondary).Height);
        var sidebarH = FindDesired(slots, map.Sidebar).Height;

        double totalH;
        if (sideBySide)
        {
            var leftH = primaryH + (map.Info is not null ? spacing + infoH : 0);
            var mainH = Math.Max(leftH, secondaryH);
            totalH = mainH + (actionH > 0 ? spacing + actionH : 0) + (sidebarH > 0 ? spacing + sidebarH : 0);
        }
        else
        {
            totalH = primaryH;
            if (map.Info is not null) totalH += spacing + infoH;
            if (map.Secondary is not null) totalH += spacing + secondaryH;
            if (actionH > 0) totalH += spacing + actionH;
            if (sidebarH > 0) totalH += spacing + sidebarH;
        }

        return new Size(available.Width, totalH);
    }

    private Size DesiredCompact(Slot[] slots, Size available)
    {
        var spacing = Spacing;
        var totalH = 0d;
        for (var i = 0; i < slots.Length; i++)
        {
            if (i > 0)
            {
                totalH += spacing;
            }

            totalH += slots[i].Control.DesiredSize.Height;
        }

        return new Size(available.Width, totalH);
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static Size FindDesired(Slot[] slots, Control? control)
    {
        if (control is null)
        {
            return default;
        }

        foreach (var slot in slots)
        {
            if (ReferenceEquals(slot.Control, control))
            {
                return control.DesiredSize;
            }
        }

        return default;
    }

    private static Rect Advance(ref double y, double x, double width, double height, double spacing)
    {
        if (y > 0)
        {
            y += spacing;
        }

        var rect = new Rect(x, y, width, height);
        y += height;
        return rect;
    }

    private static Rect MirrorHorizontally(Rect rect, double totalWidth)
    {
        return new Rect(totalWidth - rect.Right, rect.Y, rect.Width, rect.Height);
    }

    private sealed class RoleMap
    {
        public Control? Primary { get; set; }
        public Control? Secondary { get; set; }
        public Control? Info { get; set; }
        public Control? ActionPrimary { get; set; }
        public Control? ActionSecondary { get; set; }
        public Control? Sidebar { get; set; }
    }

    private readonly record struct Slot(Control Control, double Width, double AvailableHeight);
}