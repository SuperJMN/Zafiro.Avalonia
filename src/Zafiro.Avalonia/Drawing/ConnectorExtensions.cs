using Avalonia.Media;

namespace Zafiro.Avalonia.Drawing;

public static class ConnectorExtensions
{
    public static void ConnectWithSLine(this DrawingContext context, Point from, Side sideFrom, Point to, Side sideTo,
        Pen pen, bool startArrow = false, bool endArrow = false)
    {
        // Define the offset for the curve control points
        double offset = 100;
        Point controlPoint1 = from;
        Point controlPoint2 = to;

        // Adjust the control points depending on the connection side
        switch (sideFrom)
        {
            case Side.Top:
                controlPoint1 = new Point(from.X, from.Y - offset);
                break;
            case Side.Bottom:
                controlPoint1 = new Point(from.X, from.Y + offset);
                break;
            case Side.Left:
                controlPoint1 = new Point(from.X - offset, from.Y);
                break;
            case Side.Right:
                controlPoint1 = new Point(from.X + offset, from.Y);
                break;
        }

        switch (sideTo)
        {
            case Side.Top:
                controlPoint2 = new Point(to.X, to.Y - offset);
                break;
            case Side.Bottom:
                controlPoint2 = new Point(to.X, to.Y + offset);
                break;
            case Side.Left:
                controlPoint2 = new Point(to.X - offset, to.Y);
                break;
            case Side.Right:
                controlPoint2 = new Point(to.X + offset, to.Y);
                break;
        }

        // Create the Bézier curve
        var segment = new BezierSegment
        {
            Point1 = controlPoint1,
            Point2 = controlPoint2,
            Point3 = to
        };

        var figure = new PathFigure
        {
            StartPoint = from,
            Segments = new PathSegments {segment},
            IsClosed = false,
        };

        var geometry = new PathGeometry
        {
            Figures = new PathFigures {figure}
        };

        // Draw the curve
        context.DrawGeometry(null, pen, geometry);

        // Draw arrows when they are enabled
        if (startArrow)
        {
            DrawArrowHead(context, controlPoint1, from, pen);
        }

        if (endArrow)
        {
            DrawArrowHead(context, controlPoint2, to, pen);
        }
    }


    private static void DrawArrowHead(DrawingContext context, Point controlPoint, Point endPoint, Pen pen)
    {
        // Calculate the direction at the end point
        Vector direction = endPoint - controlPoint;
        direction = direction.Normalize();

        // Perpendicular vector for the base of the arrow
        Vector perpendicular = new Vector(-direction.Y, direction.X);

        // Arrowhead size
        double arrowSize = 10;

        // Points for the arrow triangle
        Point arrowPoint1 = endPoint - direction * arrowSize + perpendicular * (arrowSize / 2);
        Point arrowPoint2 = endPoint - direction * arrowSize - perpendicular * (arrowSize / 2);

        // Create the arrow shape
        var arrowFigure = new PathFigure
        {
            StartPoint = endPoint,
            Segments = new PathSegments
            {
                new LineSegment {Point = arrowPoint1},
                new LineSegment {Point = arrowPoint2}
            },
            IsClosed = true
        };

        var arrowGeometry = new PathGeometry
        {
            Figures = new PathFigures {arrowFigure}
        };

        // Draw the arrowhead
        context.DrawGeometry(pen.Brush, null, arrowGeometry);
    }

}