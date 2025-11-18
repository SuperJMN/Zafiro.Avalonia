namespace Zafiro.Avalonia.Controls.Panels;

/// <summary>
/// A panel that arranges child elements around a central element, similar to a 3x3 grid.
/// Elements can be positioned to the left, right, top, bottom, or center, where the central
/// element fills the remaining available space. Anchored elements (left, right, top, bottom)
/// do not exceed the size of the central element along their aligned axis.
/// </summary>
/// <remarks>
/// Use the <see cref="PositionProperty"/> attached property to specify the position
/// of each child. Possible positions include Left, Right, Top, Bottom, and Center.
/// The panel adjusts the dimensions of anchored elements to ensure they stay within
/// the bounds of the central element.
/// </remarks>
public class TableDockPanel : Panel
{
    public static readonly AttachedProperty<TableDock> PositionProperty =
        AvaloniaProperty.RegisterAttached<TableDockPanel, Control, TableDock>("Position", TableDock.Center);

    public static TableDock GetPosition(Control element)
    {
        return element.GetValue(PositionProperty);
    }

    public static void SetPosition(Control element, TableDock position)
    {
        element.SetValue(PositionProperty, position);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double leftWidth = 0, rightWidth = 0, topHeight = 0, bottomHeight = 0;
        double centerWidth = 0, centerHeight = 0;

        foreach (var child in Children)
        {
            var position = GetPosition(child);
            Size constraint;

            switch (position)
            {
                case TableDock.Left:
                case TableDock.Right:
                    double availableHeight = double.IsInfinity(availableSize.Height) ? double.PositiveInfinity : Math.Max(0, availableSize.Height - topHeight - bottomHeight);
                    constraint = new Size(double.PositiveInfinity, availableHeight);
                    child.Measure(constraint);
                    break;
                case TableDock.Top:
                case TableDock.Bottom:
                    double availableWidth = double.IsInfinity(availableSize.Width) ? double.PositiveInfinity : Math.Max(0, availableSize.Width - leftWidth - rightWidth);
                    constraint = new Size(availableWidth, double.PositiveInfinity);
                    child.Measure(constraint);
                    break;
                case TableDock.Center:
                    // The center will be measured later
                    break;
            }

            var desiredSize = child.DesiredSize;

            switch (position)
            {
                case TableDock.Left:
                    leftWidth = Math.Max(leftWidth, desiredSize.Width);
                    break;
                case TableDock.Right:
                    rightWidth = Math.Max(rightWidth, desiredSize.Width);
                    break;
                case TableDock.Top:
                    topHeight = Math.Max(topHeight, desiredSize.Height);
                    break;
                case TableDock.Bottom:
                    bottomHeight = Math.Max(bottomHeight, desiredSize.Height);
                    break;
            }
        }

        double centerAvailableWidth = double.IsInfinity(availableSize.Width) ? double.PositiveInfinity : Math.Max(0, availableSize.Width - leftWidth - rightWidth);
        double centerAvailableHeight = double.IsInfinity(availableSize.Height) ? double.PositiveInfinity : Math.Max(0, availableSize.Height - topHeight - bottomHeight);

        foreach (var child in Children)
        {
            if (GetPosition(child) == TableDock.Center)
            {
                var centerConstraint = new Size(centerAvailableWidth, centerAvailableHeight);
                child.Measure(centerConstraint);
                centerWidth = Math.Max(centerWidth, child.DesiredSize.Width);
                centerHeight = Math.Max(centerHeight, child.DesiredSize.Height);
            }
        }

        double totalWidth = leftWidth + centerWidth + rightWidth;
        double totalHeight = topHeight + centerHeight + bottomHeight;

        double width = double.IsInfinity(availableSize.Width) ? totalWidth : Math.Min(totalWidth, availableSize.Width);
        double height = double.IsInfinity(availableSize.Height) ? totalHeight : Math.Min(totalHeight, availableSize.Height);

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double leftWidth = 0, rightWidth = 0, topHeight = 0, bottomHeight = 0;
        double centerAvailableWidth = Math.Max(0, finalSize.Width);
        double centerAvailableHeight = Math.Max(0, finalSize.Height);

        foreach (var child in Children)
        {
            var position = GetPosition(child);
            var desiredSize = child.DesiredSize;

            switch (position)
            {
                case TableDock.Left:
                    leftWidth = Math.Max(leftWidth, desiredSize.Width);
                    break;
                case TableDock.Right:
                    rightWidth = Math.Max(rightWidth, desiredSize.Width);
                    break;
                case TableDock.Top:
                    topHeight = Math.Max(topHeight, desiredSize.Height);
                    break;
                case TableDock.Bottom:
                    bottomHeight = Math.Max(bottomHeight, desiredSize.Height);
                    break;
            }
        }

        centerAvailableWidth = Math.Max(0, finalSize.Width - leftWidth - rightWidth);
        centerAvailableHeight = Math.Max(0, finalSize.Height - topHeight - bottomHeight);

        foreach (var child in Children)
        {
            var position = GetPosition(child);
            var desiredSize = child.DesiredSize;
            Rect rect = new Rect();

            switch (position)
            {
                case TableDock.Left:
                    rect = new Rect(0, topHeight, desiredSize.Width, centerAvailableHeight);
                    break;
                case TableDock.Right:
                    rect = new Rect(finalSize.Width - desiredSize.Width, topHeight, desiredSize.Width, centerAvailableHeight);
                    break;
                case TableDock.Top:
                    rect = new Rect(leftWidth, 0, centerAvailableWidth, desiredSize.Height);
                    break;
                case TableDock.Bottom:
                    rect = new Rect(leftWidth, finalSize.Height - desiredSize.Height, centerAvailableWidth, desiredSize.Height);
                    break;
                case TableDock.Center:
                    rect = new Rect(leftWidth, topHeight, centerAvailableWidth, centerAvailableHeight);
                    break;
            }

            child.Arrange(rect);
        }

        return finalSize;
    }
}