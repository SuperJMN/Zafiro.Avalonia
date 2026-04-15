using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Zafiro.Avalonia.Controls.Diagrams;

namespace Zafiro.Avalonia.Controls.Flow;

public class FlowEditor : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable<IHaveLocation>> NodesProperty = AvaloniaProperty.Register<FlowEditor, IEnumerable<IHaveLocation>>(
        nameof(Nodes));

    public static readonly StyledProperty<IEnumerable<IHaveFromTo>> EdgesProperty = AvaloniaProperty.Register<FlowEditor, IEnumerable<IHaveFromTo>>(
        nameof(Edges));

    public static readonly StyledProperty<IDataTemplate> NodeTemplateProperty = AvaloniaProperty.Register<FlowEditor, IDataTemplate>(
        nameof(NodeTemplate));

    public static readonly StyledProperty<AvaloniaList<object>> SelectedNodesProperty = AvaloniaProperty.Register<FlowEditor, AvaloniaList<object>>(
        nameof(SelectedNodes));

    public FlowEditor()
    {
        SelectedNodes = new AvaloniaList<object>();
    }

    public IEnumerable<IHaveLocation> Nodes
    {
        get => GetValue(NodesProperty);
        set => SetValue(NodesProperty, value);
    }

    public IEnumerable<IHaveFromTo> Edges
    {
        get => GetValue(EdgesProperty);
        set => SetValue(EdgesProperty, value);
    }

    public IDataTemplate NodeTemplate
    {
        get => GetValue(NodeTemplateProperty);
        set => SetValue(NodeTemplateProperty, value);
    }

    public AvaloniaList<object> SelectedNodes
    {
        get => GetValue(SelectedNodesProperty);
        set => SetValue(SelectedNodesProperty, value);
    }
}