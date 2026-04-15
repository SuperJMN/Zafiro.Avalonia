using System.Collections.Generic;
using Avalonia.Collections;
using TestApp.Shell;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Flow;

[Section(icon: "fa-diagram-project", sortIndex: 10)]
public class FlowViewModel : ViewModelBase
{
    public FlowViewModel()
    {
        var n1 = new FlowNode("Source") { Left = 50, Top = 100, IsActive = true, ProcessingPower = 100 };
        var n2 = new FlowNode("Process") { Left = 250, Top = 100, IsActive = true, ProcessingPower = 50 };
        var n3 = new FlowNode("Sink") { Left = 450, Top = 100, IsActive = false, ProcessingPower = 0 };

        Nodes = new List<FlowNode> { n1, n2, n3 };
        Edges = new List<FlowEdge>
        {
            new FlowEdge(n1, n2),
            new FlowEdge(n2, n3)
        };

        SelectedNodes = new AvaloniaList<object>();
    }

    public List<FlowNode> Nodes { get; }
    public List<FlowEdge> Edges { get; }
    public AvaloniaList<object> SelectedNodes { get; }
}