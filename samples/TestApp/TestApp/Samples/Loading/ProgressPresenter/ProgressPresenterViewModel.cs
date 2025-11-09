using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using ReactiveUI;
using Zafiro.Progress;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Loading.ProgressPresenter;

[Section(icon: "mdi-progress-check", sortIndex: 4)]
public class ProgressPresenterViewModel : ReactiveObject
{
    private readonly SourceList<ProgressOption> optionsSource = new();
    private readonly ReadOnlyObservableCollection<ProgressOption> options;
    private ProgressOption? selectedOption;
    private string percentageLabel = "Progress";
    private string currentLabel = "Transferred";
    private string totalLabel = "Total";
    private string unitLabel = "bytes";
    private string completedLabel = "Completed";
    private string notStartedLabel = "Not started";
    private string unknownLabel = "In progress";

    public ProgressPresenterViewModel()
    {
        optionsSource.AddRange(
        [
            new ProgressOption("Not started", NotStarted.Instance),
            new ProgressOption("Unknown duration", Unknown.Instance),
            new ProgressOption("In progress (45%)", new ProportionalProgress(0.45)),
            new ProgressOption("Processing items (3 of 10)", new ProgressWithCurrentAndTotal<int>(3, 10)),
            new ProgressOption("Copying data (5.2 MB of 20 MB)", new ProgressWithCurrentAndTotal<long>(5_242_880, 20_971_520)),
            new ProgressOption("Completed", Completed.Instance)
        ]);

        optionsSource.Connect()
            .Bind(out options)
            .Subscribe();

        SelectedOption = Options.FirstOrDefault();
    }

    public ReadOnlyObservableCollection<ProgressOption> Options => options;

    public ProgressOption? SelectedOption
    {
        get => selectedOption;
        set
        {
            if (Equals(selectedOption, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref selectedOption, value);
            this.RaisePropertyChanged(nameof(SelectedProgress));
        }
    }

    public IProgress? SelectedProgress => SelectedOption?.Progress;

    public string PercentageLabel
    {
        get => percentageLabel;
        set => this.RaiseAndSetIfChanged(ref percentageLabel, value);
    }

    public string CurrentLabel
    {
        get => currentLabel;
        set => this.RaiseAndSetIfChanged(ref currentLabel, value);
    }

    public string TotalLabel
    {
        get => totalLabel;
        set => this.RaiseAndSetIfChanged(ref totalLabel, value);
    }

    public string UnitLabel
    {
        get => unitLabel;
        set => this.RaiseAndSetIfChanged(ref unitLabel, value);
    }

    public string CompletedLabel
    {
        get => completedLabel;
        set => this.RaiseAndSetIfChanged(ref completedLabel, value);
    }

    public string NotStartedLabel
    {
        get => notStartedLabel;
        set => this.RaiseAndSetIfChanged(ref notStartedLabel, value);
    }

    public string UnknownLabel
    {
        get => unknownLabel;
        set => this.RaiseAndSetIfChanged(ref unknownLabel, value);
    }
}

public record ProgressOption(string Name, IProgress Progress);
