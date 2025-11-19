using Avalonia.Media;

namespace Zafiro.Avalonia.Controls;

/// <summary>
/// A border that draws the stroke fully inside the control and paints no background under the border.
/// This avoids color blending artifacts with semi-transparent borders.
/// </summary>
public class BackgroundOverlappingBorder : Decorator
{
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<BackgroundOverlappingBorder, IBrush?>(nameof(Background));

    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<BackgroundOverlappingBorder, IBrush?>(nameof(BorderBrush));

    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<BackgroundOverlappingBorder, Thickness>(nameof(BorderThickness));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<BackgroundOverlappingBorder, CornerRadius>(nameof(CornerRadius));


    static BackgroundOverlappingBorder()
    {
        AffectsRender<BackgroundOverlappingBorder>(BackgroundProperty, BorderBrushProperty, BorderThicknessProperty, CornerRadiusProperty);
        AffectsMeasure<BackgroundOverlappingBorder>(BorderThicknessProperty, PaddingProperty);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }


    protected override Size MeasureOverride(Size availableSize)
    {
        var t = BorderThickness;
        var p = Padding;
        var inner = new Size(
            Math.Max(0, availableSize.Width - t.Left - t.Right - p.Left - p.Right),
            Math.Max(0, availableSize.Height - t.Top - t.Bottom - p.Top - p.Bottom));

        Child?.Measure(inner);
        var desired = Child?.DesiredSize ?? default;

        return new Size(
            desired.Width + t.Left + t.Right + p.Left + p.Right,
            desired.Height + t.Top + t.Bottom + p.Top + p.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var t = BorderThickness;
        var p = Padding;

        // Area inside the border (where the background is drawn)
        var innerRect = new Rect(
            t.Left,
            t.Top,
            Math.Max(0, finalSize.Width - t.Left - t.Right),
            Math.Max(0, finalSize.Height - t.Top - t.Bottom));

        // Content area (applies padding)
        var contentRect = new Rect(
            innerRect.X + p.Left,
            innerRect.Y + p.Top,
            Math.Max(0, innerRect.Width - p.Left - p.Right),
            Math.Max(0, innerRect.Height - p.Top - p.Bottom));

        Child?.Arrange(contentRect);

        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);
        var t = BorderThickness;
        var outerRR = new RoundedRect(bounds, CornerRadius);

        var innerRect = new Rect(
            t.Left,
            t.Top,
            Math.Max(0, bounds.Width - t.Left - t.Right),
            Math.Max(0, bounds.Height - t.Top - t.Bottom));

        var innerRadius = ComputeInnerRadius(CornerRadius, t);

        // Background on the full control area (so the border can overlap it)
        if (Background is { } bg)
        {
            context.DrawRectangle(bg, null, outerRR);
        }

        // Border drawn as an inner ring fully inside, overlapping the background
        if (BorderBrush is { } bb && (t.Left > 0 || t.Top > 0 || t.Right > 0 || t.Bottom > 0))
        {
            if (innerRect.Width <= 0 || innerRect.Height <= 0)
            {
                // Border covers everything
                context.DrawRectangle(bb, null, outerRR);
            }
            else
            {
                // Border as ring: outer - inner with EvenOdd
                var sg = new StreamGeometry();
                using (var gc = sg.Open())
                {
                    gc.SetFillRule(FillRule.EvenOdd);
                    AddRoundedRect(gc, bounds, CornerRadius);
                    AddRoundedRect(gc, innerRect, innerRadius);
                }

                context.DrawGeometry(bb, null, sg);
            }
        }
    }

    private static CornerRadius ComputeInnerRadius(CornerRadius r, Thickness t) =>
        new(
            Math.Max(0, r.TopLeft - Math.Max(t.Left, t.Top)),
            Math.Max(0, r.TopRight - Math.Max(t.Right, t.Top)),
            Math.Max(0, r.BottomRight - Math.Max(t.Right, t.Bottom)),
            Math.Max(0, r.BottomLeft - Math.Max(t.Left, t.Bottom)));

    private static CornerRadius ReduceRadiusByThickness(CornerRadius r, Thickness t) =>
        new(
            Math.Max(0, r.TopLeft - Math.Max(t.Left, t.Top)),
            Math.Max(0, r.TopRight - Math.Max(t.Right, t.Top)),
            Math.Max(0, r.BottomRight - Math.Max(t.Right, t.Bottom)),
            Math.Max(0, r.BottomLeft - Math.Max(t.Left, t.Bottom)));

    private static void AddRoundedRect(StreamGeometryContext ctx, Rect rect, CornerRadius radius)
    {
        // Clamp radii to half of rect dimensions
        var tl = Math.Min(radius.TopLeft, Math.Min(rect.Width, rect.Height) / 2);
        var tr = Math.Min(radius.TopRight, Math.Min(rect.Width, rect.Height) / 2);
        var br = Math.Min(radius.BottomRight, Math.Min(rect.Width, rect.Height) / 2);
        var bl = Math.Min(radius.BottomLeft, Math.Min(rect.Width, rect.Height) / 2);

        var x = rect.X;
        var y = rect.Y;
        var w = rect.Width;
        var h = rect.Height;

        // Start at top-left corner (after horizontal radius)
        ctx.BeginFigure(new Point(x + tl, y), isFilled: true);

        // Top edge to top-right corner
        ctx.LineTo(new Point(x + w - tr, y));
        if (tr > 0)
            ctx.ArcTo(new Point(x + w, y + tr), new Size(tr, tr), 0, false, SweepDirection.Clockwise);

        // Right edge to bottom-right corner
        ctx.LineTo(new Point(x + w, y + h - br));
        if (br > 0)
            ctx.ArcTo(new Point(x + w - br, y + h), new Size(br, br), 0, false, SweepDirection.Clockwise);

        // Bottom edge to bottom-left corner
        ctx.LineTo(new Point(x + bl, y + h));
        if (bl > 0)
            ctx.ArcTo(new Point(x, y + h - bl), new Size(bl, bl), 0, false, SweepDirection.Clockwise);

        // Left edge to top-left corner
        ctx.LineTo(new Point(x, y + tl));
        if (tl > 0)
            ctx.ArcTo(new Point(x + tl, y), new Size(tl, tl), 0, false, SweepDirection.Clockwise);

        ctx.EndFigure(true);
    }
}