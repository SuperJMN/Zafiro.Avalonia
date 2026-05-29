using TestApp.Samples.Shell.Hierarchy;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Shell.Hierarchy.Sections;

public abstract class HierarchicalDemoPage(string title, string summary)
{
    public string Title { get; } = title;
    public string Summary { get; } = summary;
}

[Section("workspace", "mdi-view-dashboard", 0, FriendlyName = "Workspace")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class WorkspaceViewModel : HierarchicalDemoPage
{
    public WorkspaceViewModel()
        : base("Workspace", "Operational area for day-to-day work.")
    {
    }
}

[Section("reports", "mdi-chart-box-outline", 1, FriendlyName = "Reports")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class ReportsViewModel : HierarchicalDemoPage
{
    public ReportsViewModel()
        : base("Reports", "Reporting area with its own navigation scope.")
    {
    }
}

[Section("customers", "mdi-account-group", 0, FriendlyName = "Customers", ParentId = "workspace")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class CustomersViewModel : HierarchicalDemoPage
{
    public CustomersViewModel()
        : base("Customers", "Customer operations within the workspace area.")
    {
    }
}

[Section("security", "mdi-shield-key", 1, FriendlyName = "Security", ParentId = "workspace")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class SecurityViewModel : HierarchicalDemoPage
{
    public SecurityViewModel()
        : base("Security", "Access control section under workspace.")
    {
    }
}

[Section("active-customers", "mdi-account-check", 0, FriendlyName = "Active", ParentId = "customers")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class ActiveCustomersViewModel : HierarchicalDemoPage
{
    public ActiveCustomersViewModel()
        : base("Active customers", "Leaf section with an independent navigator.")
    {
    }
}

[Section("segments", "mdi-chart-donut", 1, FriendlyName = "Segments", ParentId = "customers")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class SegmentsViewModel : HierarchicalDemoPage
{
    public SegmentsViewModel()
        : base("Segments", "Sibling leaf section under customers.")
    {
    }
}

[Section("sales", "mdi-chart-line", 0, FriendlyName = "Sales", ParentId = "reports")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class SalesViewModel : HierarchicalDemoPage
{
    public SalesViewModel()
        : base("Sales", "Reports branch leaf section.")
    {
    }
}

[Section("forecast", "mdi-calendar-clock", 1, FriendlyName = "Forecast", ParentId = "reports")]
[SectionGroup(HierarchicalShellSampleViewModel.DemoGroupKey, "Hierarchical Shell Demo")]
public class ForecastViewModel : HierarchicalDemoPage
{
    public ForecastViewModel()
        : base("Forecast", "Another reports branch leaf section.")
    {
    }
}
