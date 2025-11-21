using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TestApp.Samples.SlimDataGrid;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Layout.ResponsivePresenterSample;

[Section(icon: "mdi-tablet-cellphone", sortIndex: 10)]
[SectionGroup("layout", "Layout & Panels")]
public partial class ResponsivePresenterViewModel : ReactiveObject
{
    public ResponsivePresenterViewModel()
    {
        People = new ReadOnlyObservableCollection<Person>(new ObservableCollection<Person>([
            new() { Name = "Pepe", Surname = "Marchena" },
            new() { Name = "Carlos", Surname = "Santana" },
            new() { Name = "Lola", Surname = "Flores" },
            new() { Name = "Rafael", Surname = "Amargo" },
            new() { Name = "Juanito", Surname = "Valderrama" }
        ]));
    }

    public ReadOnlyObservableCollection<Person> People { get; }
}

