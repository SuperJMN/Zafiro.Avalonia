using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace TestApp.Samples.MasterDetails;

public partial class SampleSection : ReactiveObject
{
    [Reactive] private string title = "";
    [Reactive] private string workspace = "";
    [Reactive] private string category = "";
    [Reactive] private string status = "";
    [Reactive] private string owner = "";
    [Reactive] private string priority = "";
    [Reactive] private string summary = "";
    [Reactive] private string notes = "";
    [Reactive] private int progress;
}
