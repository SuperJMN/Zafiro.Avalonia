using CSharpFunctionalExtensions;

namespace Zafiro.Avalonia.Services;

public interface ILauncherService
{
    Task<Result> LaunchUri(Uri uri);
}