using Zafiro.Avalonia.Controls.Diagrams;

namespace TestApp.Samples.Flow;

public class FlowEdge : IHaveFromTo
{
    public FlowEdge(FlowNode from, FlowNode to)
    {
        From = from;
        To = to;
    }

    public object From { get; }
    public object To { get; }
}