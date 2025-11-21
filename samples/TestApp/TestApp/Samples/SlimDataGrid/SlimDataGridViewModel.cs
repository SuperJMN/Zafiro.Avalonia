using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.SlimDataGrid;

[Section(icon: "mdi-grid", sortIndex: 0)]
[SectionGroup("data-display", "Data Display")]
public partial class SlimDataGridViewModel : ReactiveValidationObject
{
    [Reactive] private string? personName;
    [Reactive] private string? personSurname;

    public SlimDataGridViewModel()
    {
        var sourceCache = new SourceCache<Person, (string, string)>(x => (x.Name, x.Surname));
        sourceCache.Edit(x => x.Load(GetPeople()));
        sourceCache
            .Connect()
            .Bind(out var people)
            .Subscribe();

        People = people;


        var canAdd = this.WhenAnyValue<SlimDataGridViewModel, bool, string, string>(x => x.PersonName, x => x.PersonSurname,
            (name, surname) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(surname));

        Add = ReactiveCommand.Create(() => sourceCache.AddOrUpdate(new Person { Name = personName!, Surname = personSurname! }), canAdd);

        this.ValidationRule<SlimDataGridViewModel, string>(x => x.PersonName, x => !string.IsNullOrWhiteSpace(x), "Name is required");
        this.ValidationRule<SlimDataGridViewModel, string>(x => x.PersonSurname, x => !string.IsNullOrWhiteSpace(x), "Surname is required");
    }

    public ReactiveCommand<Unit, Unit> Add { get; }

    public ReadOnlyObservableCollection<Person> People { get; }

    private static IList<Person> GetPeople()
    {
        return
        [
            new() { Name = "Pepe", Surname = "Marchena" },
            new() { Name = "Carlos", Surname = "Santana" },
            new() { Name = "Lola", Surname = "Flores" },
            new() { Name = "Rafael", Surname = "Amargo" },
            new() { Name = "Juanito", Surname = "Valderrama" }
        ];
    }
}

public class Person
{
    public required string Name { get; init; }
    public required string Surname { get; init; }
}