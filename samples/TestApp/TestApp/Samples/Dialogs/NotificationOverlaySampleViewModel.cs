using System.Reactive;
using ReactiveUI;
using Zafiro.Avalonia.Dialogs;
using Zafiro.UI;
using Zafiro.UI.Commands;

namespace TestApp.Samples.Dialogs;

[Section(icon: "mdi-bell", sortIndex: 5)]
[SectionGroup("navigation", "Navigation & Dialogs")]
public class NotificationOverlaySampleViewModel(INotificationService notificationService, IDialog dialog) : ReactiveObject, IViewModel
{
    public ReactiveCommand<Unit, Unit> ShowNotificationOverDialog { get; } = ReactiveCommand.CreateFromTask(async () =>
    {
        var dialogTask = dialog.Show(new NotificationOverlaySampleView(), "Adorner dialog", closeable =>
        [
            new Option("Close", ReactiveCommand.Create(closeable.Close).Enhance(), new Settings { Role = OptionRole.Primary, IsDefault = true })
        ]);

        await notificationService.Show("Notifications should remain bright even when the adorner dialog is open.", "Toast over adorner");
        await dialogTask;
    });
}
