using System.Collections.Generic;
using TestApp.Shell;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Diagrams.Simple;

[Section(icon: "fa-diagram-project", sortIndex: 9)]
[SectionGroup("data-viz", "Data Visualization")]
public class SimpleDiagramViewModel : ViewModelBase
{
    public SimpleDiagramViewModel()
    {
        var a = new Node("A") { Left = 50, Top = 50 };
        var b = new Node("B") { Left = 200, Top = 150d };
        var c = new Node("C") { Left = 300d, Top = 50d };
        var d = new Node("D") { Left = 450d, Top = 250d };

        Nodes = new List<Node>()
        {
            a, b, c, d
        };

        Edges = new List<Edge>()
        {
            new Edge(a, b),
            new Edge(a, c),
            new Edge(b, c),
            new Edge(c, d),
        };
    }

    public List<Edge> Edges { get; set; }
    public IEnumerable<Node> Nodes { get; }
}