using ReactiveUI.Avalonia;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CSharpFunctionalExtensions;
using ReactiveUI;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Actions;
using Zafiro.FileSystem.Mutable;
using Zafiro.UI;

namespace SampleFileExplorer.ViewModels;

public class TestViewModel
{
    private readonly BehaviorSubject<double> progressSubject = new BehaviorSubject<double>(0d);

    public TestViewModel(IMutableFileSystem fs, INotificationService notificationService)
    {
        Copy = ReactiveCommand.CreateFromTask(() =>
        {
            var file1 = fs.GetFile(
                "D:/Pel�culas/Vaiana (microHD) (EliteTorrent.net).mkv");
            var file2 = fs.GetDirectory("c:/users/jmn/Desktop").Bind(x => x.CreateFile("Copied.mkv"));

            return file1.CombineAndBind(file2, (a, b) => a.GetContents()
                .Map(data => new CopyFileAction(data, b))
                .Bind(async fileAction =>
                {
                    using (fileAction.Progress.Select(x => x.Value).Sample(TimeSpan.FromSeconds(1), AvaloniaScheduler.Instance).Subscribe(progressSubject))
                    {
                        return await fileAction.Execute().ConfigureAwait(false);
                    }
                }));
        });

        Copy.Subscribe(result => notificationService.Show(result.ToString()));
    }

    public IObservable<double> Progress => progressSubject.AsObservable();

    public ReactiveCommand<Unit, Result> Copy { get; set; }
}