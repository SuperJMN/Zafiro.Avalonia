using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls.Selection;
using ReactiveUI;
using Zafiro.Avalonia.DataViz.Graphs.Control;
using Zafiro.DataAnalysis.Clustering;
using Zafiro.DataAnalysis.Graphs;
using Zafiro.DataAnalysis.Graphs.ForceDirected;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Misc;

[Section(icon: "fa-list-check", sortIndex: 20)]
[SectionGroup("controls", "Controls & Input")]
public class EasyListBoxViewModel
{
    public EasyListBoxViewModel()
    {
        SelectionModel = new SelectionModel<Item> { SingleSelect = false };
        Items = [new(1), new(2), new(3)];
        //SelectionHandler = new SelectionHandler<Item, int>(SelectionModel, arg => arg.Id);

        var graph = new RandomGraphGenerator().GenerateRandomGraph(30, 3);

        var edge2Ds = graph.edges.Cast<IMutableEdge>();

        var graph2D = new MutableGraph(graph.nodes.Cast<IMutableNode>().ToList(), edge2Ds.ToList());

        var engine1 = new ForceDirectedEngine(graph2D);

        engine1.Distribute(5000, 3000);
        MutableGraph = graph2D;

        Play = ReactiveCommand
            .CreateFromObservable(() => Observable.Interval(TimeSpan.FromMilliseconds(12), RxApp.MainThreadScheduler).Do(_ => engine1.Step()));

        Play.Execute().Subscribe();

        var randomGraph = new RandomGraphGenerator().GenerateRandomGraph(30, 10);
        var graph2d = new MutableGraph(randomGraph.nodes.Cast<IMutableNode>().ToList(), randomGraph.edges.Cast<IMutableEdge>().ToList());

        var engine2 = new ForceDirectedEngine(graph2d);

        engine2.Distribute(2000, 2000);
        GradualGraph = new GradualGraph<IMutableNode, IMutableEdge>(graph2D, new LoadOptions());

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

    public ReactiveCommand<Unit, long> Play { get; set; }

    public List<Item> Items { get; }

    //public SelectionHandler<Item, int> SelectionHandler { get; }

    public SelectionModel<Item> SelectionModel { get; }
    public IMutableGraph MutableGraph { get; }
    public GradualGraph<IMutableNode, IMutableEdge> GradualGraph { get; }
    public ClusterNode Cluster { get; }
}