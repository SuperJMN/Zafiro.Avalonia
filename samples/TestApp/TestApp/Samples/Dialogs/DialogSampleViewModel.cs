using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.MigrateToZafiro;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.UI;
using Zafiro.UI.Shell.Utils;

namespace TestApp.Samples.Dialogs;

[Section(icon: "fa-comment-dots", sortIndex: 4)]
[SectionGroup("navigation", "Navigation & Dialogs")]
public class DialogSampleViewModel : IViewModel
{
    public DialogSampleViewModel(INotificationService notificationService, IDialog dialogService)
    {
        ShowDialog = ReactiveCommand.CreateFromTask(async () =>
        {
            return await dialogService.ShowAndGetResult(new MyViewModel(dialogService), "Dale durity", model => model.IsValid(),
                model => model.Text);
        });

        ShowDialog
            .Values()
            .SelectMany(x => TaskMixin.ToSignal(() => notificationService.Show($"You entered \"{x}\" in the dialog ;)", "Howdy!")))
            .Subscribe();

        ShowDialog
            .Empties()
            .SelectMany(_ => TaskMixin.ToSignal(() => notificationService.Show("You dismissed the dialog", "Ouch!")))
            .Subscribe();

        ShowMessage = ReactiveCommand.CreateFromTask(() => OnShowMessage(dialogService));
        WithSubmitResult = ReactiveCommand.CreateFromTask(() => dialogService.ShowAndGetResult(new SubmitterViewModel(), "My View", model => model.Submit));

        WithSubmitResult.Values()
            .SelectMany(async i =>
            {
                await notificationService.Show($"You submitted with result {i}", "Submitted!");
                return Unit.Default;
            })
            .Subscribe();
        BigDialog = ReactiveCommand.CreateFromTask(() => dialogService.Show(new BigView(), "Big", Observable.Return(true)));
    }

    public ReactiveCommand<Unit, Maybe<int>> WithSubmitResult { get; set; }

    public ReactiveCommand<Unit, Unit> BigDialog { get; set; }

    public ReactiveCommand<Unit, Unit> ShowMessage { get; set; }

    public ReactiveCommand<Unit, Maybe<string>> ShowDialog { get; set; }

    private static Task OnShowMessage(IDialog dialogService)
    {
        return dialogService.ShowMessage("Dialog Title", "Hi, this is the text of the dialog. The View is connected to the ViewModel using a DataTemplate");
    }
}