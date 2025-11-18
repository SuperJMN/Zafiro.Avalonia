using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Zafiro.DataAnalysis.Graphs;
using INode = Zafiro.FileSystem.Core.INode;

namespace Zafiro.Avalonia.Controls.Diagrams.Enhanced;

public class Edges : MarkupExtension
{
    // The list of nodes that will be referenced
    public required IEnumerable<INode> List { get; set; }

    // Collect the <avalonia:Edge .../> elements as children
    // Marked with [Content] so XAML puts EdgeItem instances inside.
    [Content]
    public List<EdgeItem> Items { get; } = new List<EdgeItem>();

    // This method is invoked at XAML time to create the "output"
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var result = new List<IEdge<INode>>();

        if (List == null || !List.Any() || Items.Count == 0)
            return result; // empty list if there is no data

        // For each "EdgeItem", find in the List the node whose Name matches
        foreach (var edgeItem in Items)
        {
            var fromNode = List.FirstOrDefault(n => n.Name == edgeItem.From);
            var toNode   = List.FirstOrDefault(n => n.Name == edgeItem.To);

            if (fromNode != null && toNode != null)
            {
                result.Add(new MyEdge(fromNode, toNode));
            }
        }
        return result;
    }

    // Internal edge implementation
    private class MyEdge : IEdge<INode>
    {
        public INode From { get; }
        public INode To { get; }

        public MyEdge(INode from, INode to)
        {
            From = from;
            To   = to;
        }
    }
}