using System.Reactive.Disposables;
using Avalonia.Input;
using Avalonia.Xaml.Interactions.Custom;
using JetBrains.Annotations;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Behaviors;

[PublicAPI]
public class TextBoxAutoSelectTextBehavior : AttachedToVisualTreeBehavior<TextBox>
{
    protected override IDisposable OnAttachedToVisualTreeOverride()
    {
        if (AssociatedObject is null)
        {
            return Disposable.Empty;
        }

        var gotFocus = AssociatedObject.OnEvent(InputElement.GotFocusEvent);
        var lostFocus = AssociatedObject.OnEvent(InputElement.LostFocusEvent);
        var isFocused = gotFocus.Select(_ => true).Merge(lostFocus.Select(_ => false));

        return isFocused
            .Throttle(TimeSpan.FromSeconds(0.1))
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Where(focused => focused)
            .Do(_ => AssociatedObject.SelectAll())
            .Subscribe();
    }
}