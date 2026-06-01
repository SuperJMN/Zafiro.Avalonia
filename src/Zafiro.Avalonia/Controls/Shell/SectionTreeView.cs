using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Zafiro.UI.Navigation.Sections;
using Zafiro.UI.Shell;

namespace Zafiro.Avalonia.Controls.Shell;

public class SectionTreeView : TreeView
{
    public static readonly StyledProperty<IHierarchicalShell?> ShellProperty =
        AvaloniaProperty.Register<SectionTreeView, IHierarchicalShell?>(nameof(Shell));

    private readonly SerialDisposable selectedPathSubscription = new();
    private readonly SerialDisposable sectionChangesSubscription = new();
    private IReadOnlyList<SectionTreeNode> nodes = [];
    private bool isUpdatingSelection;

    public SectionTreeView()
    {
        SelectionMode = SelectionMode.Single;

        this.GetObservable(ShellProperty)
            .Subscribe(UpdateShell);

        SelectionChanged += OnSelectionChanged;
        AddHandler(TreeViewItem.CollapsedEvent, OnTreeViewItemCollapsed);
    }

    public IHierarchicalShell? Shell
    {
        get => GetValue(ShellProperty);
        set => SetValue(ShellProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateShell(Shell);
        ExpandGeneratedItems();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        selectedPathSubscription.Disposable = Disposable.Empty;
        sectionChangesSubscription.Disposable = Disposable.Empty;
        base.OnDetachedFromVisualTree(e);
    }

    private static IReadOnlyList<SectionTreeNode> CreateNodes(IEnumerable<ISection> sections)
    {
        var visibleSections = sections
            .Where(section => section.IsVisible)
            .OrderBy(section => section.SortOrder)
            .ToList();

        var childrenByParentId = visibleSections
            .OfType<IHierarchicalSection>()
            .Where(section => !string.IsNullOrWhiteSpace(section.ParentId))
            .GroupBy(section => section.ParentId!)
            .ToDictionary(group => group.Key, group => group.Cast<ISection>().OrderBy(section => section.SortOrder).ToList());

        return visibleSections
            .Where(section => GetParentId(section) is null)
            .Select(section => new SectionTreeNode(section, GetChildren(section, childrenByParentId)))
            .ToList();
    }

    private static IReadOnlyList<SectionTreeNode> GetChildren(ISection section, IReadOnlyDictionary<string, List<ISection>> childrenByParentId)
    {
        return childrenByParentId.TryGetValue(section.Id, out var children)
            ? children.Select(child => new SectionTreeNode(child, [])).ToList()
            : [];
    }

    private static string? GetParentId(ISection section)
    {
        return section is IHierarchicalSection { ParentId: { } parentId } && !string.IsNullOrWhiteSpace(parentId)
            ? parentId
            : null;
    }

    private static SectionTreeNode? FindNode(IEnumerable<SectionTreeNode> candidates, ISection section)
    {
        foreach (var candidate in candidates)
        {
            if (ReferenceEquals(candidate.Section, section) || candidate.Id == section.Id)
            {
                return candidate;
            }

            var child = FindNode(candidate.Children, section);
            if (child is not null)
            {
                return child;
            }
        }

        return null;
    }

    private void UpdateShell(IHierarchicalShell? shell)
    {
        selectedPathSubscription.Disposable = Disposable.Empty;
        sectionChangesSubscription.Disposable = Disposable.Empty;

        if (shell is null)
        {
            nodes = [];
            ItemsSource = nodes;
            SelectedItem = null;
            return;
        }

        var sections = shell.Sections.ToList();
        sectionChangesSubscription.Disposable = SubscribeToSectionChanges(sections, () => RebuildNodes(shell));
        selectedPathSubscription.Disposable = shell.SelectedPath.Subscribe(UpdateSelectedPath);
        RebuildNodes(shell);
    }

    private IDisposable SubscribeToSectionChanges(IEnumerable<ISection> sections, Action rebuild)
    {
        var disposables = new CompositeDisposable();

        foreach (var section in sections)
        {
            PropertyChangedEventHandler handler = (_, args) =>
            {
                if (args.PropertyName is nameof(ISection.IsVisible) or nameof(ISection.SortOrder))
                {
                    rebuild();
                }
            };

            section.PropertyChanged += handler;
            disposables.Add(Disposable.Create(() => section.PropertyChanged -= handler));
        }

        return disposables;
    }

    private void RebuildNodes(IHierarchicalShell shell)
    {
        nodes = CreateNodes(shell.Sections);
        ItemsSource = nodes;
        UpdateSelectedPath(shell.SelectedPath.Value);
        ExpandGeneratedItems();
    }

    private void UpdateSelectedPath(IReadOnlyList<ISection> selectedPath)
    {
        isUpdatingSelection = true;
        SelectedItem = selectedPath.LastOrDefault() is { } selectedSection
            ? FindNode(nodes, selectedSection)
            : null;
        isUpdatingSelection = false;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (isUpdatingSelection || Shell is null || SelectedItem is not SectionTreeNode selectedNode)
        {
            return;
        }

        Shell.GoToSection(selectedNode.Id);
    }

    private void OnTreeViewItemCollapsed(object? sender, RoutedEventArgs e)
    {
        if (e.Source is TreeViewItem item)
        {
            item.IsExpanded = true;
            e.Handled = true;
        }
    }

    private void ExpandGeneratedItems()
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var item in this.GetVisualDescendants().OfType<TreeViewItem>())
            {
                item.IsExpanded = true;
            }
        }, DispatcherPriority.Loaded);
    }
}
