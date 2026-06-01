using System.Linq;
using System.Reactive.Linq;
using TestApp.Samples.Shell.Hierarchy;
using TestApp.Samples.Shell.Hierarchy.Sections;

namespace Zafiro.Avalonia.Tests;

public class HierarchicalShellSampleTests
{
    [Fact]
    public void TestApp_sample_defines_two_navigation_levels_from_section_attributes()
    {
        using var sample = new HierarchicalShellSampleViewModel();

        Assert.Equal(["home", "funds", "investor", "founder"], sample.Shell.RootLevel.Sections.Select(section => section.Id));
        Assert.Equal(["home"], sample.Shell.SelectedPath.Value.Select(section => section.Id));
        Assert.Empty(sample.Shell.ChildLevels.Value);

        sample.Shell.GoToSection("investor");
        Assert.Equal(["investor", "find-projects"], sample.Shell.SelectedPath.Value.Select(section => section.Id));
        Assert.Single(sample.Shell.ChildLevels.Value);
        Assert.Equal(["find-projects", "funded"], sample.Shell.ChildLevels.Value[0].Sections.Select(section => section.Id));

        sample.Shell.GoToSection("founder");
        Assert.Equal(["founder", "my-projects"], sample.Shell.SelectedPath.Value.Select(section => section.Id));
        Assert.Single(sample.Shell.ChildLevels.Value);
        Assert.Equal(["my-projects", "funders"], sample.Shell.ChildLevels.Value[0].Sections.Select(section => section.Id));
    }

    [Fact]
    public async Task TestApp_sample_keeps_each_section_navigation_scope()
    {
        using var sample = new HierarchicalShellSampleViewModel();

        sample.Shell.GoToSection("find-projects");
        var findProjectsNavigator = sample.Shell.SelectedSection.Value.Navigator;
        await findProjectsNavigator.Go(typeof(FundedViewModel));

        sample.Shell.GoToSection("my-projects");
        var myProjectsContent = await sample.Shell.SelectedSection.Value.Navigator.Content.FirstAsync();
        Assert.IsType<MyProjectsViewModel>(myProjectsContent);

        sample.Shell.GoToSection("find-projects");
        var findProjectsContent = await sample.Shell.SelectedSection.Value.Navigator.Content.FirstAsync();
        Assert.IsType<FundedViewModel>(findProjectsContent);
    }
}
