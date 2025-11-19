using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Zafiro.DataAnalysis.Clustering;

namespace Zafiro.Avalonia.DataViz.Dendrograms;

public class DendrogramLinesControl : Control
{
    public static readonly StyledProperty<ICluster?> ClusterProperty =
        AvaloniaProperty.Register<DendrogramLinesControl, ICluster?>(nameof(Cluster));

    public static readonly StyledProperty<double> LineThicknessProperty =
        AvaloniaProperty.Register<DendrogramLinesControl, double>(nameof(LineThickness), 1);

    public static readonly StyledProperty<IBrush> LineBrushProperty = AvaloniaProperty.Register<DendrogramLinesControl, IBrush>(
        nameof(LineBrush));

    static DendrogramLinesControl()
    {
        AffectsRender<DendrogramLinesControl>(ClusterProperty, LineThicknessProperty, LineBrushProperty);
    }

    public ICluster? Cluster
    {
        get => GetValue(ClusterProperty);
        set => SetValue(ClusterProperty, value);
    }

    public double LineThickness
    {
        get => GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    public IBrush LineBrush
    {
        get => GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Cluster == null)
        {
            return;
        }

        // Define the inner margin based on the line thickness
        var margin = LineThickness / 2;

        // Adjust the available drawing area
        var availableWidth = Bounds.Width - 2 * margin;
        var availableHeight = Bounds.Height - 2 * margin;

        // Define a dictionary to store the positions of the leaves
        var leafPositions = new Dictionary<ICluster, double>();
        var leafClusters = GetLeaves(Cluster).ToList();

        var leafCount = leafClusters.Count;
        var leafSpacing = leafCount > 1 ? availableWidth / (leafCount - 1) : availableWidth;

        // Assign X positions to the leaves, adjusting for the margin
        for (var i = 0; i < leafClusters.Count; i++)
        {
            var x = margin + (leafCount > 1 ? i * leafSpacing : availableWidth / 2);
            leafPositions[leafClusters[i]] = x;
        }

        // Calculate the maximum height based on MergeDistance
        var maxDistance = GetMaxMergeDistance(Cluster);

        // Create a geometry to draw the dendrogram
        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            // Draw the dendrogram lines
            DrawClusterLines(ctx, Cluster, leafPositions, margin, availableHeight, maxDistance);
        }

        // Draw the resulting geometry
        var pen = new Pen(LineBrush, LineThickness)
        {
            LineJoin = PenLineJoin.Miter,
            LineCap = PenLineCap.Flat
        };
        context.DrawGeometry(null, pen, geometry);
    }

    private void DrawClusterLines(StreamGeometryContext ctx, ICluster cluster, Dictionary<ICluster, double> leafPositions, double margin, double availableHeight, double maxDistance)
    {
        if (cluster.Left != null && cluster.Right != null)
        {
            // Calculate positions
            var leftX = GetClusterX(cluster.Left, leafPositions);
            var rightX = GetClusterX(cluster.Right, leafPositions);
            var centerX = (leftX + rightX) / 2;

            var clusterY = margin + (1 - cluster.MergeDistance / maxDistance) * availableHeight;
            var leftY = margin + (1 - cluster.Left.MergeDistance / maxDistance) * availableHeight;
            var rightY = margin + (1 - cluster.Right.MergeDistance / maxDistance) * availableHeight;

            // Build the dendrogram path
            // Start at the lower-left point
            ctx.BeginFigure(new Point(leftX, leftY), false);

            // Left vertical line going up
            ctx.LineTo(new Point(leftX, clusterY));

            // Top horizontal line from leftX to rightX
            ctx.LineTo(new Point(rightX, clusterY));

            // Right vertical line going down
            ctx.LineTo(new Point(rightX, rightY));

            // Draw the subtrees recursively
            DrawClusterLines(ctx, cluster.Left, leafPositions, margin, availableHeight, maxDistance);
            DrawClusterLines(ctx, cluster.Right, leafPositions, margin, availableHeight, maxDistance);
        }
        else if (cluster.Left == null && cluster.Right == null)
        {
            // It is a leaf; nothing needs to be drawn
        }
        else
        {
            // Handle possible nodes with a single child
            var child = cluster.Left ?? cluster.Right!;
            var childX = GetClusterX(child, leafPositions);
            var childY = margin + (1 - child.MergeDistance / maxDistance) * availableHeight;
            var clusterY = margin + (1 - cluster.MergeDistance / maxDistance) * availableHeight;

            // Start at the child point
            ctx.BeginFigure(new Point(childX, childY), false);

            // Vertical line going up
            ctx.LineTo(new Point(childX, clusterY));

            // Draw the subtree recursively
            DrawClusterLines(ctx, child, leafPositions, margin, availableHeight, maxDistance);
        }
    }

    private double GetClusterX(ICluster cluster, Dictionary<ICluster, double> leafPositions)
    {
        if (cluster.Left == null && cluster.Right == null)
        {
            // It is a leaf
            return leafPositions[cluster];
        }

        // It is an internal node
        var leftX = cluster.Left != null ? GetClusterX(cluster.Left, leafPositions) : 0;
        var rightX = cluster.Right != null ? GetClusterX(cluster.Right, leafPositions) : Bounds.Width;
        return (leftX + rightX) / 2;
    }

    private IEnumerable<ICluster> GetLeaves(ICluster cluster)
    {
        if (cluster.Left == null && cluster.Right == null)
        {
            yield return cluster;
        }
        else
        {
            if (cluster.Left != null)
            {
                foreach (var leaf in GetLeaves(cluster.Left)) yield return leaf;
            }

            if (cluster.Right != null)
            {
                foreach (var leaf in GetLeaves(cluster.Right)) yield return leaf;
            }
        }
    }

    private double GetMaxMergeDistance(ICluster cluster)
    {
        var maxDistance = cluster.MergeDistance;

        if (cluster.Left != null)
        {
            maxDistance = Math.Max(maxDistance, GetMaxMergeDistance(cluster.Left));
        }

        if (cluster.Right != null)
        {
            maxDistance = Math.Max(maxDistance, GetMaxMergeDistance(cluster.Right));
        }

        return maxDistance;
    }
}