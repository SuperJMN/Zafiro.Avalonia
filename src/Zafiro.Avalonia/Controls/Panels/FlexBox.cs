namespace Zafiro.Avalonia.Controls.Panels;

public enum FlexDirection
{
    Row,
    RowReverse,
    Column,
    ColumnReverse
}

public enum FlexWrap
{
    NoWrap,
    Wrap,
    WrapReverse
}

public enum FlexJustify
{
    Start,
    End,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

public enum FlexAlign
{
    Start,
    End,
    Center,
    Stretch,
    Baseline
}

public enum FlexAlignContent
{
    Start,
    End,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly,
    Stretch
}

public enum FlexBasisUnit
{
    Auto,
    Content,
    Pixels
}

public struct FlexBasis
{
    public FlexBasisUnit Unit { get; set; }
    public double Value { get; set; }

    public static FlexBasis Auto => new() { Unit = FlexBasisUnit.Auto };
    public static FlexBasis Content => new() { Unit = FlexBasisUnit.Content };
    public static FlexBasis Pixels(double value) => new() { Unit = FlexBasisUnit.Pixels, Value = value };

    public static implicit operator FlexBasis(double value) => Pixels(value);
}

public struct FlexValue
{
    public double Grow { get; set; }
    public double Shrink { get; set; }
    public FlexBasis Basis { get; set; }

    public static FlexValue Initial => new() { Grow = 0, Shrink = 1, Basis = FlexBasis.Auto };
    public static FlexValue Auto => new() { Grow = 1, Shrink = 1, Basis = FlexBasis.Auto };
    public static FlexValue None => new() { Grow = 0, Shrink = 0, Basis = FlexBasis.Auto };

    public static FlexValue Create(double grow = 0, double shrink = 1, FlexBasis? basis = null)
        => new() { Grow = grow, Shrink = shrink, Basis = basis ?? FlexBasis.Auto };
}

public class FlexBox : Panel
{
    // Main properties
    public static readonly StyledProperty<FlexDirection> DirectionProperty =
        AvaloniaProperty.Register<FlexBox, FlexDirection>(nameof(Direction), FlexDirection.Row);

    public static readonly StyledProperty<FlexWrap> WrapProperty =
        AvaloniaProperty.Register<FlexBox, FlexWrap>(nameof(Wrap), FlexWrap.NoWrap);

    public static readonly StyledProperty<FlexJustify> JustifyContentProperty =
        AvaloniaProperty.Register<FlexBox, FlexJustify>(nameof(JustifyContent), FlexJustify.Start);

    public static readonly StyledProperty<FlexAlign> AlignItemsProperty =
        AvaloniaProperty.Register<FlexBox, FlexAlign>(nameof(AlignItems), FlexAlign.Stretch);

    public static readonly StyledProperty<FlexAlignContent> AlignContentProperty =
        AvaloniaProperty.Register<FlexBox, FlexAlignContent>(nameof(AlignContent), FlexAlignContent.Stretch);

    public static readonly StyledProperty<double> GapProperty =
        AvaloniaProperty.Register<FlexBox, double>(nameof(Gap), 0d);

    public static readonly StyledProperty<double> RowGapProperty =
        AvaloniaProperty.Register<FlexBox, double>(nameof(RowGap), double.NaN);

    public static readonly StyledProperty<double> ColumnGapProperty =
        AvaloniaProperty.Register<FlexBox, double>(nameof(ColumnGap), double.NaN);

    // Attached properties - CSS faithful
    public static readonly AttachedProperty<FlexValue> FlexProperty =
        AvaloniaProperty.RegisterAttached<FlexBox, Control, FlexValue>("Flex", FlexValue.Initial);

    public static readonly AttachedProperty<double> GrowProperty =
        AvaloniaProperty.RegisterAttached<FlexBox, Control, double>("Grow", 0d);

    public static readonly AttachedProperty<double> ShrinkProperty =
        AvaloniaProperty.RegisterAttached<FlexBox, Control, double>("Shrink", 1d);

    public static readonly AttachedProperty<FlexBasis> BasisProperty =
        AvaloniaProperty.RegisterAttached<FlexBox, Control, FlexBasis>("Basis", FlexBasis.Auto);

    public static readonly AttachedProperty<FlexAlign?> AlignSelfProperty =
        AvaloniaProperty.RegisterAttached<FlexBox, Control, FlexAlign?>("AlignSelf", null);

    public static readonly AttachedProperty<int> OrderProperty =
        AvaloniaProperty.RegisterAttached<FlexBox, Control, int>("Order", 0);

    // Auto margin support
    public static readonly AttachedProperty<bool> MarginLeftAutoProperty =
        AvaloniaProperty.RegisterAttached<FlexBox, Control, bool>("MarginLeftAuto", false);

    public static readonly AttachedProperty<bool> MarginRightAutoProperty =
        AvaloniaProperty.RegisterAttached<FlexBox, Control, bool>("MarginRightAuto", false);

    public static readonly AttachedProperty<bool> MarginTopAutoProperty =
        AvaloniaProperty.RegisterAttached<FlexBox, Control, bool>("MarginTopAuto", false);

    public static readonly AttachedProperty<bool> MarginBottomAutoProperty =
        AvaloniaProperty.RegisterAttached<FlexBox, Control, bool>("MarginBottomAuto", false);

    static FlexBox()
    {
        AffectsMeasure<FlexBox>(DirectionProperty, WrapProperty, JustifyContentProperty,
            AlignItemsProperty, AlignContentProperty, GapProperty, RowGapProperty, ColumnGapProperty,
            FlexProperty, GrowProperty, ShrinkProperty, BasisProperty, AlignSelfProperty, OrderProperty,
            MarginLeftAutoProperty, MarginRightAutoProperty, MarginTopAutoProperty, MarginBottomAutoProperty);
    }

    // Properties
    public FlexDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public FlexWrap Wrap
    {
        get => GetValue(WrapProperty);
        set => SetValue(WrapProperty, value);
    }

    public FlexJustify JustifyContent
    {
        get => GetValue(JustifyContentProperty);
        set => SetValue(JustifyContentProperty, value);
    }

    public FlexAlign AlignItems
    {
        get => GetValue(AlignItemsProperty);
        set => SetValue(AlignItemsProperty, value);
    }

    public FlexAlignContent AlignContent
    {
        get => GetValue(AlignContentProperty);
        set => SetValue(AlignContentProperty, value);
    }

    public double Gap
    {
        get => GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    public double RowGap
    {
        get => GetValue(RowGapProperty);
        set => SetValue(RowGapProperty, value);
    }

    public double ColumnGap
    {
        get => GetValue(ColumnGapProperty);
        set => SetValue(ColumnGapProperty, value);
    }

    // Helper properties
    private bool IsRow => Direction == FlexDirection.Row || Direction == FlexDirection.RowReverse;
    private bool IsReverse => Direction == FlexDirection.RowReverse || Direction == FlexDirection.ColumnReverse;
    private bool IsWrapReverse => Wrap == FlexWrap.WrapReverse;

    private double MainGap => IsRow
        ? (double.IsNaN(ColumnGap) ? Gap : ColumnGap)
        : (double.IsNaN(RowGap) ? Gap : RowGap);

    private double CrossGap => IsRow
        ? (double.IsNaN(RowGap) ? Gap : RowGap)
        : (double.IsNaN(ColumnGap) ? Gap : ColumnGap);

    // Attached property helpers
    public static void SetFlex(Control target, FlexValue value) => target.SetValue(FlexProperty, value);
    public static FlexValue GetFlex(Control target) => target.GetValue(FlexProperty);

    public static void SetGrow(Control target, double value)
    {
        target.SetValue(GrowProperty, value);
        var flex = GetFlex(target);
        flex.Grow = value;
        target.SetValue(FlexProperty, flex);
    }

    public static double GetGrow(Control target) => target.GetValue(GrowProperty);

    public static void SetShrink(Control target, double value)
    {
        target.SetValue(ShrinkProperty, value);
        var flex = GetFlex(target);
        flex.Shrink = value;
        target.SetValue(FlexProperty, flex);
    }

    public static double GetShrink(Control target) => target.GetValue(ShrinkProperty);

    public static void SetBasis(Control target, FlexBasis value)
    {
        target.SetValue(BasisProperty, value);
        var flex = GetFlex(target);
        flex.Basis = value;
        target.SetValue(FlexProperty, flex);
    }

    public static FlexBasis GetBasis(Control target) => target.GetValue(BasisProperty);

    public static void SetAlignSelf(Control target, FlexAlign? value) => target.SetValue(AlignSelfProperty, value);
    public static FlexAlign? GetAlignSelf(Control target) => target.GetValue(AlignSelfProperty);

    public static void SetOrder(Control target, int value) => target.SetValue(OrderProperty, value);
    public static int GetOrder(Control target) => target.GetValue(OrderProperty);

    // Auto margin helpers
    public static void SetMarginLeftAuto(Control target, bool value) => target.SetValue(MarginLeftAutoProperty, value);
    public static bool GetMarginLeftAuto(Control target) => target.GetValue(MarginLeftAutoProperty);

    public static void SetMarginRightAuto(Control target, bool value) => target.SetValue(MarginRightAutoProperty, value);
    public static bool GetMarginRightAuto(Control target) => target.GetValue(MarginRightAutoProperty);

    public static void SetMarginTopAuto(Control target, bool value) => target.SetValue(MarginTopAutoProperty, value);
    public static bool GetMarginTopAuto(Control target) => target.GetValue(MarginTopAutoProperty);

    public static void SetMarginBottomAuto(Control target, bool value) => target.SetValue(MarginBottomAutoProperty, value);
    public static bool GetMarginBottomAuto(Control target) => target.GetValue(MarginBottomAutoProperty);

    protected override Size MeasureOverride(Size availableSize)
    {
        // Ignore invisible children when calculating the layout
        var orderedChildren = Children.Where(c => c.IsVisible).OrderBy(GetOrder).ToList();

        // CSS faithful measurement with proper intrinsic sizing
        foreach (var child in orderedChildren)
        {
            var basis = GetEffectiveBasis(child);
            var measureSize = basis.Unit switch
            {
                FlexBasisUnit.Auto => availableSize,
                FlexBasisUnit.Content => Size.Infinity,
                FlexBasisUnit.Pixels => IsRow
                    ? new Size(basis.Value, availableSize.Height)
                    : new Size(availableSize.Width, basis.Value),
                _ => availableSize
            };
            child.Measure(measureSize);
        }

        var containerMain = IsRow ? availableSize.Width : availableSize.Height;
        var lines = CreateFlexLines(orderedChildren, containerMain);

        // Handle align-content for measurement
        var totalMain = lines.Count > 0 ? lines.Max(l => l.MainSize) : 0;
        var totalCross = CalculateTotalCrossSize(lines);

        return IsRow
            ? new Size(totalMain, totalCross)
            : new Size(totalCross, totalMain);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Ignore invisible children when calculating the layout
        var orderedChildren = Children.Where(c => c.IsVisible).OrderBy(GetOrder).ToList();
        if (!orderedChildren.Any()) return finalSize;

        var containerMain = IsRow ? finalSize.Width : finalSize.Height;
        var containerCross = IsRow ? finalSize.Height : finalSize.Width;
        var lines = CreateFlexLines(orderedChildren, containerMain);

        // CSS Flexbox Algorithm Steps:
        // 1. Resolve flexible lengths
        foreach (var line in lines)
        {
            ResolveFlexibleLengths(line, containerMain);
        }

        // 2. Handle auto margins
        foreach (var line in lines)
        {
            HandleAutoMargins(line, containerMain);
        }

        // 3. Align items within lines
        var crossPositions = CalculateCrossPositions(lines, containerCross);

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            var line = lines[lineIndex];
            var crossPos = crossPositions[lineIndex];

            ArrangeLine(line, lineIndex, containerMain, crossPos);
        }

        return finalSize;
    }

    private FlexBasis GetEffectiveBasis(Control child)
    {
        var flex = GetFlex(child);
        if (flex.Basis.Unit != FlexBasisUnit.Auto)
            return flex.Basis;

        var basis = GetBasis(child);
        return basis.Unit != FlexBasisUnit.Auto ? basis : FlexBasis.Auto;
    }

    private List<FlexLine> CreateFlexLines(List<Control> children, double containerMain)
    {
        var lines = new List<FlexLine>();
        var currentLine = new FlexLine();

        foreach (var child in children)
        {
            var item = CreateFlexItem(child);
            var gap = currentLine.Items.Any() ? MainGap : 0;

            // Check wrapping - CSS faithful
            if (Wrap != FlexWrap.NoWrap && currentLine.Items.Any())
            {
                var wouldOverflow = currentLine.MainSize + gap + item.MainSize > containerMain;
                if (wouldOverflow)
                {
                    lines.Add(currentLine);
                    currentLine = new FlexLine();
                    gap = 0;
                }
            }

            currentLine.AddItem(item, gap);
        }

        if (currentLine.Items.Any())
            lines.Add(currentLine);

        // Handle wrap-reverse
        if (IsWrapReverse)
            lines.Reverse();

        return lines;
    }

    private FlexItem CreateFlexItem(Control control)
    {
        var flex = GetFlex(control);
        var size = control.DesiredSize;
        var basis = GetEffectiveBasis(control);

        // Calculate main and cross sizes based on flex-basis
        double mainSize = basis.Unit switch
        {
            FlexBasisUnit.Auto => IsRow ? size.Width : size.Height,
            FlexBasisUnit.Content => IsRow ? size.Width : size.Height, // Simplified
            FlexBasisUnit.Pixels => basis.Value,
            _ => IsRow ? size.Width : size.Height
        };

        double crossSize = IsRow ? size.Height : size.Width;

        return new FlexItem
        {
            Control = control,
            MainSize = mainSize,
            CrossSize = crossSize,
            OriginalMainSize = mainSize,
            Grow = flex.Grow > 0 ? flex.Grow : GetGrow(control),
            Shrink = flex.Shrink >= 0 ? flex.Shrink : GetShrink(control),
            Basis = basis,
            MarginMainStart = GetAutoMargin(control, true, true),
            MarginMainEnd = GetAutoMargin(control, true, false),
            MarginCrossStart = GetAutoMargin(control, false, true),
            MarginCrossEnd = GetAutoMargin(control, false, false)
        };
    }

    private bool GetAutoMargin(Control control, bool isMainAxis, bool isStart)
    {
        if (IsRow)
        {
            if (isMainAxis)
                return isStart ? GetMarginLeftAuto(control) : GetMarginRightAuto(control);
            else
                return isStart ? GetMarginTopAuto(control) : GetMarginBottomAuto(control);
        }
        else
        {
            if (isMainAxis)
                return isStart ? GetMarginTopAuto(control) : GetMarginBottomAuto(control);
            else
                return isStart ? GetMarginLeftAuto(control) : GetMarginRightAuto(control);
        }
    }

    private void ResolveFlexibleLengths(FlexLine line, double containerMain)
    {
        var totalMainSize = line.Items.Sum(i => i.MainSize) + MainGap * Math.Max(0, line.Items.Count - 1);
        var remaining = containerMain - totalMainSize;

        if (remaining > 0)
        {
            // Distribute positive free space (flex-grow)
            var totalGrow = line.Items.Sum(i => i.Grow);
            if (totalGrow > 0)
            {
                foreach (var item in line.Items)
                {
                    if (item.Grow > 0)
                    {
                        var extraSpace = remaining * (item.Grow / totalGrow);
                        item.MainSize += extraSpace;
                    }
                }
            }
        }
        else if (remaining < 0)
        {
            // Distribute negative free space (flex-shrink)
            var deficit = Math.Abs(remaining);
            var totalShrink = line.Items.Sum(i => i.Shrink * i.OriginalMainSize);

            if (totalShrink > 0)
            {
                foreach (var item in line.Items)
                {
                    if (item.Shrink > 0)
                    {
                        var shrinkRatio = (item.Shrink * item.OriginalMainSize) / totalShrink;
                        var shrinkAmount = Math.Min(deficit * shrinkRatio, item.MainSize);
                        item.MainSize -= shrinkAmount;
                    }
                }
            }
        }

        // Update line main size
        line.MainSize = line.Items.Sum(i => i.MainSize) + MainGap * Math.Max(0, line.Items.Count - 1);
    }

    private void HandleAutoMargins(FlexLine line, double containerMain)
    {
        var remaining = containerMain - line.MainSize;
        if (remaining <= 0) return;

        var autoMarginItems = line.Items.Where(i => i.MarginMainStart || i.MarginMainEnd).ToList();
        if (!autoMarginItems.Any()) return;

        var totalAutoMargins = autoMarginItems.Sum(i =>
            (i.MarginMainStart ? 1 : 0) + (i.MarginMainEnd ? 1 : 0));

        if (totalAutoMargins > 0)
        {
            var marginSpace = remaining / totalAutoMargins;
            foreach (var item in autoMarginItems)
            {
                if (item.MarginMainStart) item.AutoMarginMainStart = marginSpace;
                if (item.MarginMainEnd) item.AutoMarginMainEnd = marginSpace;
            }
        }
    }

    private List<double> CalculateCrossPositions(List<FlexLine> lines, double containerCross)
    {
        if (lines.Count <= 1) return new List<double> { 0 };

        var totalLinesCross = lines.Sum(l => l.CrossSize);
        var usedCross = totalLinesCross + CrossGap * (lines.Count - 1);
        var remainingCross = containerCross - usedCross;

        var (startOffset, spacing) = AlignContent switch
        {
            FlexAlignContent.End => (remainingCross, 0),
            FlexAlignContent.Center => (remainingCross / 2, 0),
            FlexAlignContent.SpaceBetween => lines.Count > 1 ? (0, remainingCross / (lines.Count - 1)) : (0, 0),
            FlexAlignContent.SpaceAround => (remainingCross / lines.Count / 2, remainingCross / lines.Count),
            FlexAlignContent.SpaceEvenly => (remainingCross / (lines.Count + 1), remainingCross / (lines.Count + 1)),
            FlexAlignContent.Stretch => (0, 0), // Handle stretch separately
            _ => (0, 0) // Start
        };

        var positions = new List<double>();
        var currentPos = startOffset;

        for (int i = 0; i < lines.Count; i++)
        {
            positions.Add(currentPos);
            currentPos += lines[i].CrossSize + spacing + (i < lines.Count - 1 ? CrossGap : 0);
        }

        return positions;
    }

    private void ArrangeLine(FlexLine line, int lineIndex, double containerMain, double crossPos)
    {
        var remainingMain = containerMain - line.MainSize;
        var (mainStart, mainSpacing) = GetJustifyContentOffset(remainingMain, line.Items.Count, lineIndex);

        var currentMainPos = mainStart;
        var items = IsReverse ? line.Items.AsEnumerable().Reverse() : line.Items;

        foreach (var item in items)
        {
            currentMainPos += item.AutoMarginMainStart;

            var alignSelf = GetAlignSelf(item.Control) ?? AlignItems;
            var (crossOffset, crossSize) = GetAlignItemsOffset(alignSelf, item, line.CrossSize);

            var finalMainPos = currentMainPos;
            var finalCrossPos = crossPos + crossOffset + item.AutoMarginCrossStart;

            // Handle baseline alignment
            if (alignSelf == FlexAlign.Baseline)
            {
                finalCrossPos = crossPos + CalculateBaselineOffset(item, line);
            }

            var rect = IsRow
                ? new Rect(finalMainPos, finalCrossPos, item.MainSize, crossSize)
                : new Rect(finalCrossPos, finalMainPos, crossSize, item.MainSize);

            item.Control.Arrange(rect);

            currentMainPos += item.MainSize + item.AutoMarginMainEnd + mainSpacing;
            if (items.ToList().IndexOf(item) < line.Items.Count - 1)
            {
                currentMainPos += MainGap;
            }
        }
    }

    private (double offset, double spacing) GetJustifyContentOffset(double remaining, int itemCount, int lineIndex = 0)
    {
        // CRITICAL FIX: For wrapped lines with SpaceBetween, treat subsequent lines as FlexEnd
        if (JustifyContent == FlexJustify.SpaceBetween && Wrap != FlexWrap.NoWrap && lineIndex > 0)
        {
            return (remaining, 0); // Align to end for wrapped lines
        }

        return JustifyContent switch
        {
            FlexJustify.End => (remaining, 0),
            FlexJustify.Center => (remaining / 2, 0),
            FlexJustify.SpaceBetween => itemCount > 1 ? (0, remaining / (itemCount - 1)) : (remaining, 0),
            FlexJustify.SpaceAround => (remaining / itemCount / 2, remaining / itemCount),
            FlexJustify.SpaceEvenly => (remaining / (itemCount + 1), remaining / (itemCount + 1)),
            _ => (0, 0) // Start
        };
    }

    private (double offset, double size) GetAlignItemsOffset(FlexAlign align, FlexItem item, double lineCross)
    {
        return align switch
        {
            FlexAlign.End => (lineCross - item.CrossSize, item.CrossSize),
            FlexAlign.Center => ((lineCross - item.CrossSize) / 2, item.CrossSize),
            FlexAlign.Stretch when !item.MarginCrossStart && !item.MarginCrossEnd => (0, lineCross),
            FlexAlign.Baseline => (0, item.CrossSize), // Calculated separately
            _ => (0, item.CrossSize) // Start
        };
    }

    private double CalculateBaselineOffset(FlexItem item, FlexLine line)
    {
        // Simplified baseline calculation - would need proper typography metrics
        if (item.Control is TextBlock textBlock)
        {
            // Estimate baseline as 80% of text height
            return 0; // Simplified - proper implementation would calculate font metrics
        }

        return 0;
    }

    private double CalculateTotalCrossSize(List<FlexLine> lines)
    {
        if (!lines.Any()) return 0;
        return lines.Sum(l => l.CrossSize) + CrossGap * Math.Max(0, lines.Count - 1);
    }

    // Helper classes
    private class FlexLine
    {
        public List<FlexItem> Items { get; } = new();
        public double MainSize { get; set; }
        public double CrossSize { get; set; }

        public void AddItem(FlexItem item, double gap)
        {
            Items.Add(item);
            MainSize += item.MainSize + gap;
            CrossSize = Math.Max(CrossSize, item.CrossSize);
        }
    }

    private class FlexItem
    {
        public Control Control { get; set; } = null!;
        public double MainSize { get; set; }
        public double CrossSize { get; set; }
        public double OriginalMainSize { get; set; }
        public double Grow { get; set; }
        public double Shrink { get; set; }
        public FlexBasis Basis { get; set; }

        // Auto margin support
        public bool MarginMainStart { get; set; }
        public bool MarginMainEnd { get; set; }
        public bool MarginCrossStart { get; set; }
        public bool MarginCrossEnd { get; set; }

        public double AutoMarginMainStart { get; set; }
        public double AutoMarginMainEnd { get; set; }
        public double AutoMarginCrossStart { get; set; }
        public double AutoMarginCrossEnd { get; set; }
    }
}