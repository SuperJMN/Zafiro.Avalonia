using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Misc;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Commands;

public class CopyParameterToClipboard(INotificationService notificationService) : EnhancedCommandWrapper<string, Result>(ReactiveCommand.CreateFromTask<string, Result>(async str =>
{
    return await ApplicationUtils.GetClipboard()
        .ToResult("Cannot access clipboard")
        .Tap(clipboard => clipboard.SetTextAsync(str))
        .Tap(() => notificationService.Show(null!, "Copied to clipboard"));
}).Enhance());