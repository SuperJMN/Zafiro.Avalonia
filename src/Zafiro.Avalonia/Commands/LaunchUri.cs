using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Services;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Commands;

public class LaunchUri(ILauncherService launcherService) : EnhancedCommandWrapper<Uri, Result>(ReactiveCommand.CreateFromTask<Uri, Result>(str => Result.Try(() => launcherService.LaunchUri(str))).Enhance());