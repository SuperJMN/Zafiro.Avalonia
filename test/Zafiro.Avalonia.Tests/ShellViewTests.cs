using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Reactive.Bindings;
using Zafiro.Avalonia.Controls;
using Zafiro.Avalonia.Controls.Shell;
using Zafiro.UI.Navigation.Sections;
using Zafiro.UI.Shell;

namespace Zafiro.Avalonia.Tests;

public class ShellViewTests
{
    private static readonly object StylesLock = new();

    [AvaloniaFact]
    public void Desktop_sidebar_filters_hidden_root_sections()
    {
        EnsureStyles();

        var visible = new SimpleSection
        {
            Id = "visible",
            FriendlyName = "Visible",
            Content = "Visible content",
        };

        var hidden = new SimpleSection
        {
            Id = "hidden",
            FriendlyName = "Hidden",
            Content = "Hidden content",
            IsVisible = false,
        };

        var shell = new TestShell([visible, hidden], visible);
        var view = Layout(new ShellView { DataContext = shell }, 900, 600);

        var text = view.GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(textBlock => textBlock is { IsVisible: true, Bounds.Width: > 0 })
            .Select(textBlock => textBlock.Text)
            .ToList();

        Assert.Contains("Visible", text);
        Assert.DoesNotContain("Hidden", text);
    }

    [AvaloniaFact]
    public void Desktop_sidebar_shows_all_second_level_sections()
    {
        EnsureStyles();

        var home = new SimpleSection
        {
            Id = "home",
            FriendlyName = "Home",
            Content = "Home content",
        };

        var investor = new SimpleSection
        {
            Id = "investor",
            FriendlyName = "Investor",
            Content = "Investor content",
        };

        var findProjects = new SimpleSection
        {
            Id = "find-projects",
            ParentId = "investor",
            FriendlyName = "Find Projects",
            Content = "Find Projects content",
        };

        var funded = new SimpleSection
        {
            Id = "funded",
            ParentId = "investor",
            FriendlyName = "Funded",
            Content = "Funded content",
        };

        var founder = new SimpleSection
        {
            Id = "founder",
            FriendlyName = "Founder",
            Content = "Founder content",
        };

        var myProjects = new SimpleSection
        {
            Id = "my-projects",
            ParentId = "founder",
            FriendlyName = "My Projects",
            Content = "My Projects content",
        };

        var funders = new SimpleSection
        {
            Id = "funders",
            ParentId = "founder",
            FriendlyName = "Funders",
            Content = "Funders content",
        };

        var shell = new TestShell([home, investor, findProjects, funded, founder, myProjects, funders], findProjects);
        var view = Layout(new ShellView { DataContext = shell }, 900, 600);

        var text = VisibleText(view);

        Assert.Contains("Find Projects", text);
        Assert.Contains("Funded", text);
        Assert.Contains("My Projects", text);
        Assert.Contains("Funders", text);
    }

    [AvaloniaFact]
    public void Shell_content_tracks_selected_child_section_when_toggled_again()
    {
        EnsureStyles();

        var investor = new SimpleSection
        {
            Id = "investor",
            FriendlyName = "Investor",
            Content = "Investor content",
        };

        var findProjects = new SimpleSection
        {
            Id = "find-projects",
            ParentId = "investor",
            FriendlyName = "Find Projects",
            Content = "Find Projects content",
        };

        var funded = new SimpleSection
        {
            Id = "funded",
            ParentId = "investor",
            FriendlyName = "Funded",
            Content = "Funded content",
        };

        var shell = new TestShell([investor, findProjects, funded], findProjects);
        var view = Layout(new ShellView { DataContext = shell }, 900, 600);

        shell.GoToSection("funded");
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("Funded content", VisibleText(view));

        shell.GoToSection("find-projects");
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("Find Projects content", VisibleText(view));

        shell.GoToSection("funded");
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("Funded content", VisibleText(view));
    }

    [AvaloniaFact]
    public void Desktop_sidebar_parent_with_children_selects_child_node_only()
    {
        EnsureStyles();

        var investor = new SimpleSection
        {
            Id = "investor",
            FriendlyName = "Investor",
            Content = "Investor content",
        };

        var findProjects = new SimpleSection
        {
            Id = "find-projects",
            ParentId = "investor",
            FriendlyName = "Find Projects",
            Content = "Find Projects content",
        };

        var funded = new SimpleSection
        {
            Id = "funded",
            ParentId = "investor",
            FriendlyName = "Funded",
            Content = "Funded content",
        };

        var shell = new TestShell([investor, findProjects, funded], findProjects);
        var view = Layout(new ShellView { DataContext = shell }, 900, 600);

        var parentItem = TreeItemFor(view, "investor");
        var selectedChildItem = TreeItemFor(view, "find-projects");

        Assert.False(parentItem.IsSelected);
        Assert.True(selectedChildItem.IsSelected);
    }

    [AvaloniaFact]
    public void Desktop_sidebar_root_without_children_selects_root_node()
    {
        EnsureStyles();

        var home = new SimpleSection
        {
            Id = "home",
            FriendlyName = "Home",
            Content = "Home content",
        };

        var shell = new TestShell([home], home);
        var view = Layout(new ShellView { DataContext = shell }, 900, 600);

        var rootItem = TreeItemFor(view, "home");

        Assert.True(rootItem.IsSelected);
    }

    [AvaloniaFact]
    public void Desktop_sidebar_child_selection_navigates_and_refreshes_content()
    {
        EnsureStyles();

        var investor = new SimpleSection
        {
            Id = "investor",
            FriendlyName = "Investor",
            Content = "Investor content",
        };

        var findProjects = new SimpleSection
        {
            Id = "find-projects",
            ParentId = "investor",
            FriendlyName = "Find Projects",
            Content = "Find Projects content",
        };

        var funded = new SimpleSection
        {
            Id = "funded",
            ParentId = "investor",
            FriendlyName = "Funded",
            Content = "Funded content",
        };

        var shell = new TestShell([investor, findProjects, funded], findProjects);
        var view = Layout(new ShellView { DataContext = shell }, 900, 600);
        var tree = Assert.Single(view.GetVisualDescendants().OfType<SectionTreeView>());

        tree.SelectedItem = TreeNodeFor(view, "funded");
        Dispatcher.UIThread.RunJobs();
        Assert.Same(funded, shell.SelectedSection.Value);
        Assert.Contains("Funded content", VisibleText(view));

        tree.SelectedItem = TreeNodeFor(view, "find-projects");
        Dispatcher.UIThread.RunJobs();
        Assert.Same(findProjects, shell.SelectedSection.Value);
        Assert.Contains("Find Projects content", VisibleText(view));
    }

    private static ShellView Layout(ShellView view, double width, double height)
    {
        var window = new Window
        {
            Content = view,
            Width = width,
            Height = height,
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        view.ApplyTemplate();
        view.Measure(new Size(width, height));
        view.Arrange(new Rect(0, 0, width, height));
        Dispatcher.UIThread.RunJobs();

        return view;
    }

    private static IReadOnlyList<string?> VisibleText(Visual visual)
    {
        return visual.GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(textBlock => textBlock is { IsVisible: true, Bounds.Width: > 0 })
            .Select(textBlock => textBlock.Text)
            .ToList();
    }

    private static TreeViewItem TreeItemFor(Visual visual, string sectionId)
    {
        return Assert.Single(visual.GetVisualDescendants()
            .OfType<TreeViewItem>()
            .Where(item => item.DataContext is SectionTreeNode node && node.Id == sectionId));
    }

    private static SectionTreeNode TreeNodeFor(Visual visual, string sectionId)
    {
        var item = TreeItemFor(visual, sectionId);
        return Assert.IsType<SectionTreeNode>(item.DataContext);
    }

    private static void EnsureStyles()
    {
        lock (StylesLock)
        {
            var styles = Application.Current!.Styles;

            if (!styles.OfType<FluentTheme>().Any())
            {
                styles.Insert(0, new FluentTheme());
            }

            if (!styles.OfType<StyleInclude>().Any(include => include.Source?.ToString() == "avares://Zafiro.Avalonia/Styles.axaml"))
            {
                styles.Add(new StyleInclude(new Uri("avares://Zafiro.Avalonia/"))
                {
                    Source = new Uri("avares://Zafiro.Avalonia/Styles.axaml"),
                });
            }
        }
    }

    private sealed class TestShell : IHierarchicalShell
    {
        public TestShell(IReadOnlyList<ISection> sections, ISection selectedSection, IReadOnlyList<SectionLevel>? childLevels = null)
        {
            Sections = sections;
            SelectedSection = new ReactiveProperty<ISection>(selectedSection);
            SelectedPath = new ReactiveProperty<IReadOnlyList<ISection>>(BuildPath(selectedSection));
            RootLevel = new SectionLevel(sections.Where(section => section is not IHierarchicalSection hierarchicalSection || string.IsNullOrWhiteSpace(hierarchicalSection.ParentId)).ToList(), selectedSection, Select);
            ChildLevels = new ReactiveProperty<IReadOnlyList<SectionLevel>>(childLevels ?? []);
        }

        public IEnumerable<ISection> Sections { get; }
        public ReactiveProperty<ISection> SelectedSection { get; }
        public SectionLevel RootLevel { get; }
        public ReactiveProperty<IReadOnlyList<SectionLevel>> ChildLevels { get; }
        public ReactiveProperty<IReadOnlyList<ISection>> SelectedPath { get; }

        public void GoToSection(string sectionId)
        {
            Select(Sections.Single(section => section.Id == sectionId));
        }

        private void Select(ISection section)
        {
            SelectedSection.Value = section;
            SelectedPath.Value = BuildPath(section);
        }

        private IReadOnlyList<ISection> BuildPath(ISection section)
        {
            var sectionsById = Sections.ToDictionary(candidate => candidate.Id);
            var path = new Stack<ISection>();
            var current = section;

            while (true)
            {
                path.Push(current);

                if (current is not IHierarchicalSection { ParentId: { } parentId } || string.IsNullOrWhiteSpace(parentId) || !sectionsById.TryGetValue(parentId, out var parent))
                {
                    return path.ToList();
                }

                current = parent;
            }
        }
    }
}
