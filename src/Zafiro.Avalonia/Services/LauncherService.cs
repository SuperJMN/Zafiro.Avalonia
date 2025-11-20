using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Services;

public class LauncherService : ILauncherService
{
    public async Task<Result> LaunchUri(Uri uri)
    {
        return await ApplicationUtils.TopLevel().ToResult("Cannot get the top level host")
            .Map(topLevel => topLevel.Launcher)
            .EnsureNotNull("The top level launcher service cannot be null")
            .Bind(l => Result.Try(() => l.LaunchUriAsync(uri)))
            .Ensure(b => b, "Launch URI operation failed").ConfigureAwait(false);
    }
}