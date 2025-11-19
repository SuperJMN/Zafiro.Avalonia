using Avalonia.Layout;

namespace Zafiro.Avalonia.Controls.Panels;

public enum TrueCenterDock
{
    Left,
    Center,
    Right
}

public class TrueCenterPanel : Panel
{
    public static readonly AttachedProperty<TrueCenterDock> DockProperty =
        AvaloniaProperty.RegisterAttached<TrueCenterPanel, Control, TrueCenterDock>(
            "Dock",
            defaultValue: TrueCenterDock.Left
        );

    public static TrueCenterDock GetDock(Visual element)
        => element.GetValue(DockProperty);

    public static void SetDock(Visual element, TrueCenterDock value)
        => element.SetValue(DockProperty, value);

    protected override Size MeasureOverride(Size availableSize)
    {
        var leftChild = Children.FirstOrDefault(c => GetDock(c) == TrueCenterDock.Left);
        var centerChild = Children.FirstOrDefault(c => GetDock(c) == TrueCenterDock.Center);
        var rightChild = Children.FirstOrDefault(c => GetDock(c) == TrueCenterDock.Right);

        // 1) Measure Left and Right freely with the offered size
        leftChild?.Measure(availableSize);
        var leftSize = leftChild?.DesiredSize ?? new Size();

        rightChild?.Measure(availableSize);
        var rightSize = rightChild?.DesiredSize ?? new Size();

        // 2) side = maximum width between left and right
        double side = Math.Max((double)leftSize.Width, rightSize.Width);

        // 3) The width available for the center = total - 2*side (clamped to >= 0)
        double centerAvailWidth = Math.Max(0, availableSize.Width - 2 * side);

        // Measure the center with that width
        if (centerChild != null)
        {
            centerChild.Measure(new Size(centerAvailWidth, availableSize.Height));
        }

        var centerSize = centerChild?.DesiredSize ?? new Size();

        // 4) The required height = the maximum among the three
        double neededHeight = new double[] { leftSize.Height, centerSize.Height, rightSize.Height }.Max();
        double finalHeight = Math.Min(neededHeight, availableSize.Height);

        double neededWidth = centerChild != null
            ? centerSize.Width + 2 * side
            : leftSize.Width + rightSize.Width;

        double finalWidth = double.IsInfinity(availableSize.Width)
            ? neededWidth
            : Math.Min(neededWidth, availableSize.Width);

        return new Size(finalWidth, finalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var leftChild = Children.FirstOrDefault(c => GetDock(c) == TrueCenterDock.Left);
        var centerChild = Children.FirstOrDefault(c => GetDock(c) == TrueCenterDock.Center);
        var rightChild = Children.FirstOrDefault(c => GetDock(c) == TrueCenterDock.Right);

        var leftSize = leftChild?.DesiredSize ?? new Size();
        var centerSize = centerChild?.DesiredSize ?? new Size();
        var rightSize = rightChild?.DesiredSize ?? new Size();

        // side = larger width between Left and Right
        double side = Math.Max(leftSize.Width, rightSize.Width);

        // --- LEFT ---
        if (leftChild != null)
        {
            double w = Math.Min(leftSize.Width, finalSize.Width);
            // Calculate the final height it will use
            double h = GetArrangedHeight(leftChild, finalSize.Height, leftSize.Height);
            // Calculate the top based on VerticalAlignment
            double top = GetTop(leftChild, finalSize.Height, h);

            leftChild.Arrange(new Rect(0, top, w, h));
        }

        // --- RIGHT ---
        if (rightChild != null)
        {
            double w = Math.Min(rightSize.Width, finalSize.Width);
            double x = Math.Max(0, finalSize.Width - w);

            double h = GetArrangedHeight(rightChild, finalSize.Height, rightSize.Height);
            double top = GetTop(rightChild, finalSize.Height, h);

            rightChild.Arrange(new Rect(x, top, w, h));
        }

        // --- CENTER ---
        if (centerChild != null)
        {
            // Central strip = [side .. (finalSize.Width - side)]
            double sliceWidth = Math.Max(0, finalSize.Width - 2 * side);
            double cWidth = Math.Min(centerSize.Width, sliceWidth);

            double cHeight = GetArrangedHeight(centerChild, finalSize.Height, centerSize.Height);
            double top = GetTop(centerChild, finalSize.Height, cHeight);

            // Center within that strip
            double cX = side + (sliceWidth - cWidth) / 2;
            centerChild.Arrange(new Rect(cX, top, cWidth, cHeight));
        }

        return finalSize;
    }

    /// <summary>
    /// Calculates the vertical position (Top) according to the control's VerticalAlignment.
    /// </summary>
    private double GetTop(Control child, double containerHeight, double arrangedHeight)
    {
        return child.VerticalAlignment switch
        {
            VerticalAlignment.Top => 0,
            VerticalAlignment.Bottom => containerHeight - arrangedHeight,
            VerticalAlignment.Center => (containerHeight - arrangedHeight) / 2,
            VerticalAlignment.Stretch => 0, // and the height takes the full space
            _ => 0
        };
    }

    /// <summary>
    /// Calculates the final height the child should occupy according to its VerticalAlignment.
    /// </summary>
    private double GetArrangedHeight(Control child, double containerHeight, double measuredHeight)
    {
        return child.VerticalAlignment == VerticalAlignment.Stretch
            ? containerHeight
            : measuredHeight;
    }
}