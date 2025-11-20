using System.Reactive;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Services;

namespace Zafiro.Avalonia.Misc;

public class Link
{
    public Link(ILauncherService lanLauncherService, string url)
    {
        Url = url;
        Open = ReactiveCommand.CreateFromTask(() => lanLauncherService.LaunchUri(new Uri(url)));
    }

    public string Url { get; }

    public object Icon { get; set; }

    public ReactiveCommand<Unit, Result> Open { get; }
}