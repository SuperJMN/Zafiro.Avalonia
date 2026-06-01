using TestApp.Samples.Shell.Hierarchy;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Shell.Hierarchy.Sections;

public abstract class HierarchicalDemoPage(string title, string summary)
{
    public string Title { get; } = title;
    public string Summary { get; } = summary;
}

[Section("home", "mdi-home", 0, FriendlyName = "Home")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class HomeViewModel : HierarchicalDemoPage
{
    public HomeViewModel()
        : base("Home", "Landing section for the investment workspace.")
    {
    }
}

[Section("funds", "mdi-wallet", 1, FriendlyName = "Funds")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class FundsViewModel : HierarchicalDemoPage
{
    public FundsViewModel()
        : base("Funds", "Browse funds and investment pools.")
    {
    }
}

[Section("investor", "mdi-account-tie", 2, FriendlyName = "Investor")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class InvestorViewModel : HierarchicalDemoPage
{
    public InvestorViewModel()
        : base("Investor", "Investor area with project discovery and funded work.")
    {
    }
}

[Section("find-projects", "mdi-magnify", 0, FriendlyName = "Find Projects", ParentId = "investor")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class FindProjectsViewModel : HierarchicalDemoPage
{
    public FindProjectsViewModel()
        : base("Find Projects", "Search opportunities from the investor branch.")
    {
    }
}

[Section("funded", "mdi-check-circle", 1, FriendlyName = "Funded", ParentId = "investor")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class FundedViewModel : HierarchicalDemoPage
{
    public FundedViewModel()
        : base("Funded", "Investor branch section with an independent navigator.")
    {
    }
}

[Section("founder", "mdi-lightbulb-on-outline", 3, FriendlyName = "Founder")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class FounderViewModel : HierarchicalDemoPage
{
    public FounderViewModel()
        : base("Founder", "Founder area for project and funder management.")
    {
    }
}

[Section("my-projects", "mdi-folder-open", 0, FriendlyName = "My Projects", ParentId = "founder")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class MyProjectsViewModel : HierarchicalDemoPage
{
    public MyProjectsViewModel()
        : base("My Projects", "Founder branch section with an independent navigator.")
    {
    }
}

[Section("funders", "mdi-handshake", 1, FriendlyName = "Funders", ParentId = "founder")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class FundersViewModel : HierarchicalDemoPage
{
    public FundersViewModel()
        : base("Funders", "Founder branch sibling section.")
    {
    }
}
