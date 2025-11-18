using Zafiro.Avalonia.Controls.Diagrams;
using Zafiro.DataAnalysis.Graphs;

namespace TestApp.Samples.Flow;

public class FlowEdge : IEdge<object>, IHaveFromTo
{
    public FlowEdge(FlowNode from, FlowNode to)
    {
        From = from;
        To = to;
    }

    public object From { get; }
    public object To { get; }
}