using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using DynamicData;
using ReactiveUI;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Misc;

[Section("ItemId Extension", icon: "fa-list-ol", sortIndex: 22)]
[SectionGroup("controls", "Controls & Input")]
public class ItemIndexViewModel
{
    public ItemIndexViewModel()
    {
        IEnumerable<NameItem> nameItems = [new NameItem("Item 1"), new NameItem("Item 2"), new NameItem("Item 3")];
        var source = new SourceCache<NameItem, string>(item => item.Name);
        source.Connect().Bind(out var items).Subscribe();
        Items = items;

        Add = ReactiveCommand.Create(() => source.AddOrUpdate(new NameItem($"Item {Random.Shared.Next()}")));
        Remove = ReactiveCommand.Create(() => source!.Remove(source.Items.FirstOrDefault()));
    }

    public ReactiveCommand<Unit, Unit> Remove { get; set; }

    public ReactiveCommand<Unit, Unit> Add { get; }

    public IEnumerable<NameItem> Items { get; }
}

public record NameItem(string Name);