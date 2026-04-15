using System.Collections.Generic;
using Avalonia.Collections;
using TestApp.Shell;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.PropertyGrid;

[Section(icon: "fa-list", sortIndex: 11)]
public class PropertyGridSampleViewModel : ViewModelBase
{
    public PropertyGridSampleViewModel()
    {
        var p1 = new Person { Name = "Alice", Age = 30, IsMember = true, Role = Role.Admin };
        var p2 = new Person { Name = "Bob", Age = 25, IsMember = false, Role = Role.User };
        var p3 = new Person { Name = "Charlie", Age = 35, IsMember = true, Role = Role.User };

        People = new List<Person> { p1, p2, p3 };
        SelectedPeople = new AvaloniaList<object>();
    }

    public List<Person> People { get; }
    public AvaloniaList<object> SelectedPeople { get; }
}

public class Person
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public bool IsMember { get; set; }
    public Role Role { get; set; }
}

public enum Role
{
    User,
    Admin,
    Guest
}