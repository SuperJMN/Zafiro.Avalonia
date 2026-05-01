using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.MasterDetails;

[Section(name: "MasterDetailsView", icon: "mdi-view-split-vertical", sortIndex: 15)]
[SectionGroup("navigation", "Navigation & Dialogs")]
public partial class MasterDetailsSampleViewModel : ReactiveObject
{
    private const string ProductWorkspace = "Product";
    private string selectedWorkspace = ProductWorkspace;

    [Reactive] private SampleSection? selectedSection;

    public MasterDetailsSampleViewModel()
    {
        Workspaces = new List<string>
        {
            ProductWorkspace,
            "Operations",
        };

        Sections = new List<SampleSection>
        {
            new()
            {
                Title = "Checkout",
                Workspace = ProductWorkspace,
                Category = "Experience",
                Status = "In review",
                Owner = "Nora",
                Priority = "High",
                Summary = "Payment confirmation, receipt delivery, and recovery paths.",
                Notes = "Keep payment state visible after transient gateway failures. Confirm the retry copy with support before release.",
                Progress = 72,
            },
            new()
            {
                Title = "Account setup",
                Workspace = ProductWorkspace,
                Category = "Onboarding",
                Status = "Ready",
                Owner = "Leo",
                Priority = "Medium",
                Summary = "Profile creation, defaults, and first-run checklist.",
                Notes = "The setup defaults are stable. Legal still needs to approve the regional terms copy.",
                Progress = 100,
            },
            new()
            {
                Title = "Release notes",
                Workspace = ProductWorkspace,
                Category = "Content",
                Status = "Draft",
                Owner = "Mara",
                Priority = "Low",
                Summary = "Version highlights and customer-facing change summaries.",
                Notes = "Long-form content belongs in the detail pane; the master keeps only scanning metadata.",
                Progress = 35,
            },
            new()
            {
                Title = "Incident queue",
                Workspace = "Operations",
                Category = "Support",
                Status = "Open",
                Owner = "Iris",
                Priority = "High",
                Summary = "Live operational work with owner and status visible in the master.",
                Notes = "The queue is above threshold. Pull the latest response-time numbers before the shift handoff.",
                Progress = 48,
            },
            new()
            {
                Title = "Deployment checklist",
                Workspace = "Operations",
                Category = "Release",
                Status = "Blocked",
                Owner = "Sami",
                Priority = "High",
                Summary = "Preflight checks, rollout gates, and post-release validation.",
                Notes = "This item keeps operational context separate while reusing the same templates.",
                Progress = 61,
            },
            new()
            {
                Title = "Billing audit",
                Workspace = "Operations",
                Category = "Finance",
                Status = "Scheduled",
                Owner = "Vera",
                Priority = "Medium",
                Summary = "Monthly verification of invoices, credits, and export status.",
                Notes = "Exports are scheduled for the first business day after reconciliation closes.",
                Progress = 20,
            },
        };

        SelectedSection = VisibleSections.FirstOrDefault();
    }

    public IReadOnlyList<string> Workspaces { get; }

    public IReadOnlyList<SampleSection> Sections { get; }

    public IEnumerable<SampleSection> VisibleSections => Sections.Where(section => section.Workspace == selectedWorkspace);

    public string MasterHeader => $"{selectedWorkspace} sections";

    public string FooterText => $"{VisibleSections.Count()} items";

    public string? SelectedWorkspace
    {
        get => selectedWorkspace;
        set
        {
            var nextWorkspace = value ?? ProductWorkspace;

            if (nextWorkspace == selectedWorkspace)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref selectedWorkspace, nextWorkspace);
            this.RaisePropertyChanged(nameof(VisibleSections));
            this.RaisePropertyChanged(nameof(MasterHeader));
            this.RaisePropertyChanged(nameof(FooterText));
            SelectedSection = VisibleSections.FirstOrDefault();
        }
    }
}
