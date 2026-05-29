using System.Linq;
using System.Reactive.Linq;
using TestApp.Samples.Shell.Hierarchy;
using TestApp.Samples.Shell.Hierarchy.Sections;

namespace Zafiro.Avalonia.Tests;

public class HierarchicalShellSampleTests
{
    [Fact]
    public void TestApp_sample_defines_three_navigation_levels_from_section_attributes()
    {
        using var sample = new HierarchicalShellSampleViewModel();

        Assert.Equal(["workspace", "reports"], sample.Shell.RootLevel.Sections.Select(section => section.Id));
        Assert.Equal(["workspace", "customers", "active-customers"], sample.Shell.SelectedPath.Value.Select(section => section.Id));
        Assert.Equal(2, sample.Shell.ChildLevels.Value.Count);
        Assert.Equal(["customers", "security"], sample.Shell.ChildLevels.Value[0].Sections.Select(section => section.Id));
        Assert.Equal(["active-customers", "segments"], sample.Shell.ChildLevels.Value[1].Sections.Select(section => section.Id));
    }

    [Fact]
    public async Task TestApp_sample_keeps_each_section_navigation_scope()
    {
        using var sample = new HierarchicalShellSampleViewModel();

        var activeCustomersNavigator = sample.Shell.SelectedSection.Value.Navigator;
        await activeCustomersNavigator.Go(typeof(SegmentsViewModel));

        sample.Shell.GoToSection("security");
        var securityContent = await sample.Shell.SelectedSection.Value.Navigator.Content.FirstAsync();
        Assert.IsType<SecurityViewModel>(securityContent);

        sample.Shell.GoToSection("active-customers");
        var activeCustomersContent = await sample.Shell.SelectedSection.Value.Navigator.Content.FirstAsync();
        Assert.IsType<SegmentsViewModel>(activeCustomersContent);
    }
}
