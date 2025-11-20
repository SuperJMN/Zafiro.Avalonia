using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Misc;
using Zafiro.Avalonia.Services;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia;

public class Commands
{
    private static Commands instance = CreateDefault();
    private readonly ILauncherService launcherService;
    private readonly INotificationService notificationService;

    public Commands(INotificationService notificationService, ILauncherService launcherService)
    {
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(launcherService);

        this.notificationService = notificationService;
        this.launcherService = launcherService;

        CopyParameter = ReactiveCommand.CreateFromTask<string, Result>(CopyParameterImplementation).Enhance();
        LaunchUri = ReactiveCommand.CreateFromTask<Uri, Result>(uri => this.launcherService.LaunchUri(uri)).Enhance();
        LaunchUriString = ReactiveCommand.CreateFromTask<string, Result>(uri => ToUri(uri).Bind(u => this.launcherService.LaunchUri(u))).Enhance();
    }

    public IEnhancedCommand<string, Result> LaunchUriString { get; }

    public static Commands Instance
    {
        get => instance;
        set => instance = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IEnhancedCommand<string, Result> CopyParameter { get; }

    public IEnhancedCommand<Uri, Result> LaunchUri { get; }

    private static Result<Uri> ToUri(string uriString)
    {
        return Uri.TryCreate(uriString, UriKind.Absolute, out var uri) ? Result.Success(uri) : Result.Failure<Uri>($"Cannot parse URI '{uriString}'");
    }

    private async Task<Result> CopyParameterImplementation(string text)
    {
        return await ApplicationUtils.GetClipboard()
            .ToResult("Cannot access clipboard")
            .Tap(clipboard => clipboard.SetTextAsync(text))
            .Tap(() => notificationService.Show(null!, "Copied to clipboard"));
    }

    private static Commands CreateDefault()
    {
        return new Commands(new DummyNotificationService(), new DummyLauncherService());
    }

    private class DummyLauncherService : ILauncherService
    {
        public Task<Result> LaunchUri(Uri uri)
        {
            return Task.FromResult(Result.Success());
        }
    }
}