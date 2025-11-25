using System;
using System.Reactive;
using System.Reactive.Linq;
using CSharpFunctionalExtensions;
using ReactiveUI;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.DivineBytes;
using Zafiro.UI;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Misc;

[Section("Storage", icon: "fa-database", sortIndex: 5)]
[SectionGroup("misc", "Miscellaneous")]
public class StorageSampleViewModel
{
    public StorageSampleViewModel(IFileSystemPicker storage)
    {
        OpenFile = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await storage.PickForOpen(new FileTypeFilter("All files", ["*.jpg", "*.png", "*.gif", "*.bmp"]));

            return result.Map(maybe => maybe.Map(file => file)).GetValueOrDefault();
        });

        var files = OpenFile.Values().Publish().RefCount();

        SelectedPaths = files.Select(file => file.Name);
        SelectedBytes = files.SelectMany(file => file.Bytes);
    }

    public IObservable<byte[]> SelectedBytes { get; set; }

    public IObservable<string> SelectedPaths { get; }

    public ReactiveCommand<Unit, Maybe<INamedByteSource>> OpenFile { get; }
}