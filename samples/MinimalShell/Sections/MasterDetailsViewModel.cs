using ReactiveUI;
using Zafiro.UI.Shell.Utils;

namespace MinimalShell.Sections;

[Section(name: "MasterDetails", icon: "fa-circle-info", sortIndex: 4)]
public class MasterDetailsViewModel : ReactiveObject
{
    private MasterDetailsItem? selectedItem;

    public MasterDetailsViewModel()
    {
        Items =
        [
            new("Planning", "Open", "Scope the work and lock the acceptance checks."),
            new("Build", "Active", "Implement the smallest useful surface for desktop and compact layouts."),
            new("Validation", "Ready", "Exercise selection, activation, and frame back behavior."),
        ];

        selectedItem = Items[0];
    }

    public IReadOnlyList<MasterDetailsItem> Items { get; }

    public MasterDetailsItem? SelectedItem
    {
        get => selectedItem;
        set => this.RaiseAndSetIfChanged(ref selectedItem, value);
    }
}

public record MasterDetailsItem(string Title, string Status, string Description);
