using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Zafiro.Avalonia.Controls.Navigation;
using Zafiro.UI.Navigation.Sections;
using Zafiro.UI.Shell;

namespace Zafiro.Avalonia.Controls.Shell;

public class ShellFrame : Frame
{
    public static readonly StyledProperty<IHierarchicalShell?> ShellProperty =
        AvaloniaProperty.Register<ShellFrame, IHierarchicalShell?>(nameof(Shell));

    private readonly SerialDisposable selectedSectionSubscription = new();
    private readonly SerialDisposable contentSubscription = new();

    public ShellFrame()
    {
        this.GetObservable(ShellProperty)
            .Subscribe(UpdateShell);
    }

    public IHierarchicalShell? Shell
    {
        get => GetValue(ShellProperty);
        set => SetValue(ShellProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateShell(Shell);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        selectedSectionSubscription.Disposable = Disposable.Empty;
        contentSubscription.Disposable = Disposable.Empty;
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateShell(IHierarchicalShell? shell)
    {
        selectedSectionSubscription.Disposable = Disposable.Empty;
        contentSubscription.Disposable = Disposable.Empty;

        if (shell is null)
        {
            Content = null;
            BackCommand = null!;
            return;
        }

        selectedSectionSubscription.Disposable = shell.SelectedSection.Subscribe(ShowSection);
    }

    private void ShowSection(ISection section)
    {
        BackCommand = section.Navigator.Back;
        contentSubscription.Disposable = section.Navigator.Content.Subscribe(ShowContent);
    }

    private void ShowContent(object? content)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Content = content;
            return;
        }

        Dispatcher.UIThread.Post(() => Content = content);
    }
}
