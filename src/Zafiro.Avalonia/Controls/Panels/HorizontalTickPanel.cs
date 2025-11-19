namespace Zafiro.Avalonia.Controls.Panels;

public class HorizontalTickPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        double maxChildHeight = 0;
        double totalChildWidth = 0;

        foreach (var child in Children)
        {
            // Measure each child with infinite width and the available height
            child.Measure(new Size(Double.PositiveInfinity, availableSize.Height));

            // Accumulate the total width and the maximum height
            totalChildWidth += child.DesiredSize.Width;
            maxChildHeight = Math.Max(maxChildHeight, child.DesiredSize.Height);
        }

        // Calculate the desired size of the panel
        double desiredWidth = double.IsInfinity(availableSize.Width) ? totalChildWidth : availableSize.Width;
        double desiredHeight = double.IsInfinity(availableSize.Height) ? maxChildHeight : availableSize.Height;

        // Return the desired size without infinite or NaN values
        return new Size(desiredWidth, desiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int count = Children.Count;
        if (count == 0)
            return finalSize;

        for (int i = 0; i < count; i++)
        {
            var child = Children[i];
            double childId = i;

            double xPosition;
            if (count == 1)
            {
                // If there is only one child, center it in the middle of the panel
                xPosition = finalSize.Width / 2;
            }
            else
            {
                // Distribute the children between 0 and finalSize.Width
                xPosition = (finalSize.Width / (count - 1)) * childId;
            }

            // Adjust to horizontally align the child relative to xPosition
            double childWidth = child.DesiredSize.Width;
            double x = xPosition - (childWidth / 2);

            // Do not clamp the x position so children can extend beyond the panel if needed
            child.Arrange(new Rect(new Point(x, 0), new Size(childWidth, finalSize.Height)));
        }

        return finalSize;
    }
}