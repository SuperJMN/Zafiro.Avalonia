using Zafiro.DataAnalysis.Clustering;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.DataAnalysis.Dendrograms;

[Section(icon: "mdi-family-tree", sortIndex: 2)]
[SectionGroup("data-viz", "Data Visualization")]
public class DendrogramViewModel
{
    public DendrogramViewModel()
    {
        Cluster<string> cluster = new Internal<string>(
            new Internal<string>(
                new Internal<string>(
                    new Leaf<string>("A"),
                    new Leaf<string>("B"), 1),
                new Leaf<string>("F"), 2),
            new Internal<string>(
                new Leaf<string>("C"),
                new Internal<string>(
                    new Leaf<string>("D"),
                    new Leaf<string>("E"), 4), 5), 7);
        Cluster = ClusterNode.Create(cluster);
    }

    public ClusterNode Cluster { get; set; }
}