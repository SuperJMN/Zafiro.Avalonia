using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace Zafiro.Avalonia.Dialogs.Behaviors;

public class AddClassesForRoleBehavior : Behavior<Control>
{
    public static readonly StyledProperty<OptionRole> RoleProperty = AvaloniaProperty.Register<AddClassesForRoleBehavior, OptionRole>(
        nameof(Role));

    private IDisposable? subscription;

    public OptionRole Role
    {
        get => GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    protected override void OnAttached()
    {
        if (AssociatedObject is null)
        {
            return;
        }

        base.OnAttached();

        subscription = this.GetObservable(RoleProperty)
            .StartWith(Role)
            .Subscribe(UpdateClasses);
    }

    protected override void OnDetaching()
    {
        subscription?.Dispose();
        base.OnDetaching();
    }

    private void UpdateClasses(OptionRole role)
    {
        if (AssociatedObject is null) return;

        AssociatedObject.Classes.Remove("Primary");
        AssociatedObject.Classes.Remove("Secondary");
        AssociatedObject.Classes.Remove("Destructive");
        AssociatedObject.Classes.Remove("Hollow");

        switch (role)
        {
            case OptionRole.Primary:
                AssociatedObject.Classes.Add("Primary");
                break;
            case OptionRole.Secondary:
                AssociatedObject.Classes.Add("Secondary");
                break;
            case OptionRole.Destructive:
                AssociatedObject.Classes.Add("Destructive");
                break;
            case OptionRole.Cancel:
                AssociatedObject.Classes.Add("Hollow");
                break;
        }
    }
}