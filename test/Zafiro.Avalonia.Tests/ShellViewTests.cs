using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
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
    public void Desktop_sidebar_parent_with_children_does_not_paint_selection_behind_children()
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
            FriendlyName = "Find Projects",
            Content = "Find Projects content",
        };

        var funded = new SimpleSection
        {
            Id = "funded",
            FriendlyName = "Funded",
            Content = "Funded content",
        };

        var childLevel = new SectionLevel([findProjects, funded], findProjects, _ => { });
        var shell = new TestShell([investor], investor, [childLevel]);
        var view = Layout(new ShellView { DataContext = shell }, 900, 600);

        var parentItem = Assert.Single(view.GetVisualDescendants().OfType<SectionStripItem>().Where(item => ReferenceEquals(item.DataContext, investor)));
        var parentHeader = Assert.Single(parentItem.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("DesktopRootHeader")));
        var groupSelection = Assert.Single(parentHeader.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("DesktopRootHeaderGroupSelection")));
        var leafSelection = Assert.Single(parentHeader.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("DesktopRootHeaderLeafSelection")));
        var selectedChildItem = Assert.Single(view.GetVisualDescendants().OfType<SectionStripItem>().Where(item => ReferenceEquals(item.DataContext, findProjects)));

        Assert.Equal(Brushes.Transparent, parentItem.Background);
        Assert.True(groupSelection.IsVisible);
        Assert.False(leafSelection.IsVisible);
        Assert.NotEqual(Brushes.Transparent, groupSelection.Background);
        Assert.NotEqual(groupSelection.Background, selectedChildItem.Background);
    }

    [AvaloniaFact]
    public void Desktop_sidebar_root_without_children_keeps_accent_selection()
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

        var rootItem = Assert.Single(view.GetVisualDescendants().OfType<SectionStripItem>().Where(item => ReferenceEquals(item.DataContext, home)));
        var rootHeader = Assert.Single(rootItem.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("DesktopRootHeader")));
        var groupSelection = Assert.Single(rootHeader.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("DesktopRootHeaderGroupSelection")));
        var leafSelection = Assert.Single(rootHeader.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("DesktopRootHeaderLeafSelection")));

        Assert.Equal(Brushes.Transparent, rootItem.Background);
        Assert.False(groupSelection.IsVisible);
        Assert.True(leafSelection.IsVisible);
        Assert.NotEqual(Brushes.Transparent, leafSelection.Background);
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
            SelectedPath = new ReactiveProperty<IReadOnlyList<ISection>>([selectedSection]);
            RootLevel = new SectionLevel(sections, selectedSection, Select);
            ChildLevels = new ReactiveProperty<IReadOnlyList<SectionLevel>>(childLevels ?? []);
        }

        public IEnumerable<ISection> Sections { get; }
        public ReactiveProperty<ISection> SelectedSection { get; }
        public SectionLevel RootLevel { get; }
        public ReactiveProperty<IReadOnlyList<SectionLevel>> ChildLevels { get; }
        public ReactiveProperty<IReadOnlyList<ISection>> SelectedPath { get; }

        public void GoToSection(string sectionId)
        {
        }

        private void Select(ISection section)
        {
            SelectedSection.Value = section;
            SelectedPath.Value = [section];
        }
    }
}
