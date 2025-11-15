using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Misc;
using Zafiro.Avalonia.Services;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia;

public class Commands
{
    private readonly INotificationService notificationService;
    private readonly ILauncherService launcherService;

    private static Commands instance = CreateDefault();

    public Commands(INotificationService notificationService, ILauncherService launcherService)
    {
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(launcherService);

        this.notificationService = notificationService;
        this.launcherService = launcherService;

        CopyParameter = ReactiveCommand.CreateFromTask<string, Result>(CopyParameterImplementation).Enhance();
        LaunchUri = ReactiveCommand.CreateFromTask<Uri, Result>(LaunchUriImplementation).Enhance();
    }

    public static Commands Instance
    {
        get => instance;
        set => instance = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IEnhancedCommand<string, Result> CopyParameter { get; }

    public IEnhancedCommand<Uri, Result> LaunchUri { get; }

    private async Task<Result> CopyParameterImplementation(string text)
    {
        return await ApplicationUtils.GetClipboard()
            .ToResult("Cannot access clipboard")
            .Tap(clipboard => clipboard.SetTextAsync(text))
            .Tap(() => notificationService.Show(null!, "Copied to clipboard"));
    }

    private Task<Result> LaunchUriImplementation(Uri uri)
    {
        return Result.Try(() => launcherService.LaunchUri(uri));
    }

    private static Commands CreateDefault()
    {
        return new Commands(new DummyNotificationService(), new DummyLauncherService());
    }

    private class DummyLauncherService : ILauncherService
    {
        public Task LaunchUri(Uri uri)
        {
            return Task.CompletedTask;
        }
    }
}
