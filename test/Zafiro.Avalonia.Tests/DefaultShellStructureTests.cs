using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using MinimalShell.Sections;
using Zafiro.Avalonia.Controls.Shell;
using Zafiro.UI.Navigation;
using Zafiro.UI.Navigation.Sections;
using Zafiro.UI.Shell;

namespace Zafiro.Avalonia.Tests;

public class DefaultShellStructureTests
{
    [Fact]
    public void MinimalShell_opens_with_two_section_levels_for_investment_app_structure()
    {
        using var provider = new ServiceCollection()
            .AddZafiroShell()
            .AddSectionsFromAttributes(typeof(HomeViewModel).Assembly)
            .BuildServiceProvider();

        var shell = provider.GetRequiredService<IHierarchicalShell>();

        Assert.Equal(["home", "funds", "investor", "founder"], shell.RootLevel.Sections.Select(section => section.Id));
        Assert.Equal(["home"], shell.SelectedPath.Value.Select(section => section.Id));
        Assert.Empty(shell.ChildLevels.Value);

        shell.GoToSection("investor");
        Assert.Equal(["investor", "find-projects"], shell.SelectedPath.Value.Select(section => section.Id));
        Assert.Single(shell.ChildLevels.Value);
        Assert.Equal(["find-projects", "funded"], shell.ChildLevels.Value[0].Sections.Select(section => section.Id));

        shell.GoToSection("founder");
        Assert.Equal(["founder", "my-projects"], shell.SelectedPath.Value.Select(section => section.Id));
        Assert.Single(shell.ChildLevels.Value);
        Assert.Equal(["my-projects", "funders"], shell.ChildLevels.Value[0].Sections.Select(section => section.Id));
    }

    [Fact]
    public void ZafiroShellTemplate_defines_a_two_level_investment_app_section_tree()
    {
        var app = ReadTemplateFile("ZafiroShellTemplate", "App.axaml.cs");
        var desktopProgram = ReadTemplateFile("ZafiroShellTemplate.Desktop", "Program.cs");
        var desktopProject = ReadTemplateFile("ZafiroShellTemplate.Desktop", "ZafiroShellTemplate.Desktop.csproj");
        var home = ReadTemplateFile("ZafiroShellTemplate", "Sections", "HomeViewModel.cs");
        var funds = ReadTemplateFile("ZafiroShellTemplate", "Sections", "FundsViewModel.cs");
        var investor = ReadTemplateFile("ZafiroShellTemplate", "Sections", "InvestorViewModel.cs");
        var findProjects = ReadTemplateFile("ZafiroShellTemplate", "Sections", "FindProjectsViewModel.cs");
        var funded = ReadTemplateFile("ZafiroShellTemplate", "Sections", "FundedViewModel.cs");
        var founder = ReadTemplateFile("ZafiroShellTemplate", "Sections", "FounderViewModel.cs");
        var myProjects = ReadTemplateFile("ZafiroShellTemplate", "Sections", "MyProjectsViewModel.cs");
        var funders = ReadTemplateFile("ZafiroShellTemplate", "Sections", "FundersViewModel.cs");

        Assert.Contains("GetRequiredService<IHierarchicalShell>()", app);
        Assert.Contains(".UseMcpDiagnosticsIfDebug()", desktopProgram);
        Assert.DoesNotContain("#if DEBUG", desktopProgram);
        Assert.Contains("Zafiro.Avalonia.Mcp.AppHost", desktopProject);
        Assert.Contains("""[Section("home", "fa-home", 0, FriendlyName = "Home")]""", home);
        Assert.Contains("""[Section("funds", "fa-wallet", 1, FriendlyName = "Funds")]""", funds);
        Assert.Contains("""[Section("investor", "fa-user", 2, FriendlyName = "Investor")]""", investor);
        Assert.Contains("""[Section("find-projects", "fa-search", 0, FriendlyName = "Find Projects", ParentId = "investor")]""", findProjects);
        Assert.Contains("""[Section("funded", "fa-circle-check", 1, FriendlyName = "Funded", ParentId = "investor")]""", funded);
        Assert.Contains("""[Section("founder", "fa-lightbulb", 3, FriendlyName = "Founder")]""", founder);
        Assert.Contains("""[Section("my-projects", "fa-folder-open", 0, FriendlyName = "My Projects", ParentId = "founder")]""", myProjects);
        Assert.Contains("""[Section("funders", "fa-handshake", 1, FriendlyName = "Funders", ParentId = "founder")]""", funders);
    }

    [Fact]
    public void Shell_child_level_selector_is_visible_only_when_it_has_multiple_sections()
    {
        var converter = ShellConverters.HasMultipleSections;

        Assert.False((bool)converter.Convert(Array.Empty<ISection>(), typeof(bool), null, CultureInfo.InvariantCulture)!);
        Assert.False((bool)converter.Convert(new ISection[] { new TestSection("funded") }, typeof(bool), null, CultureInfo.InvariantCulture)!);
        Assert.True((bool)converter.Convert(new ISection[] { new TestSection("find-projects"), new TestSection("funded") }, typeof(bool), null, CultureInfo.InvariantCulture)!);
    }

    [Fact]
    public void Shell_sidebar_only_nests_the_active_first_child_level()
    {
        var home = new TestSection("home");
        var investor = new TestSection("investor");
        var findProjects = new TestSection("find-projects");
        var funded = new TestSection("funded");
        var childLevel = new SectionLevel([findProjects, funded], findProjects, _ => { });

        var firstChildLevel = ShellConverters.FirstChildLevel.Convert(new[] { childLevel }, typeof(SectionLevel), null, CultureInfo.InvariantCulture);
        var showInvestorChildren = ShellConverters.ShouldShowSelectedChildLevel.Convert(new object?[] { investor, investor, new[] { childLevel } }, typeof(bool), null, CultureInfo.InvariantCulture);
        var showHomeChildren = ShellConverters.ShouldShowSelectedChildLevel.Convert(new object?[] { home, investor, new[] { childLevel } }, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Same(childLevel, firstChildLevel);
        Assert.True((bool)showInvestorChildren!);
        Assert.False((bool)showHomeChildren!);
    }

    private static string ReadTemplateFile(params string[] segments)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var templatePath = Path.Combine(
                [
                    directory.FullName,
                    "templates",
                    "Zafiro.Avalonia.Templates",
                    "templates",
                    "zafiro-shell",
                    .. segments,
                ]);

            if (File.Exists(templatePath))
            {
                return File.ReadAllText(templatePath);
            }
        }

        throw new FileNotFoundException($"Could not find template file '{Path.Combine(segments)}'.");
    }

    private sealed class TestSection(string id) : ISection
    {
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { }
            remove { }
        }
        public bool IsVisible { get; set; } = true;
        public int SortOrder { get; set; }
        public string Id { get; } = id;
        public string? ShortName => FriendlyName;
        public string FriendlyName => Id;
        public SectionGroup Group { get; } = new();
        public object? Icon => null;
        public INavigator Navigator => null!;

        public void Dispose()
        {
        }
    }
}
