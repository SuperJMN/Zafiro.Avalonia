using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Zafiro.Avalonia.Misc;

internal static class Extensions
{
    public static Uri? GetContextBaseUri(this IServiceProvider ctx)
    {
        return ctx.GetService<IUriContext>()?.BaseUri;
    }

    private static T? GetService<T>(this IServiceProvider sp)
    {
        return (T?)sp.GetService(typeof(T));
    }

    public static bool IsButtonPressed(this PointerPointProperties pointerPoint, MouseButton mouseButton)
    {
        return mouseButton switch
        {
            MouseButton.None => false,
            MouseButton.Left => pointerPoint.IsLeftButtonPressed,
            MouseButton.Right => pointerPoint.IsRightButtonPressed,
            MouseButton.Middle => pointerPoint.IsMiddleButtonPressed,
            MouseButton.XButton1 => pointerPoint.IsXButton1Pressed,
            MouseButton.XButton2 => pointerPoint.IsXButton2Pressed,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static bool WasButtonReleased(this PointerPointProperties pointerPoint, MouseButton mouseButton)
    {
        return mouseButton switch
        {
            MouseButton.None => false,
            MouseButton.Left => pointerPoint.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased,
            MouseButton.Right => pointerPoint.PointerUpdateKind == PointerUpdateKind.RightButtonReleased,
            MouseButton.Middle => pointerPoint.PointerUpdateKind == PointerUpdateKind.MiddleButtonReleased,
            MouseButton.XButton1 => pointerPoint.PointerUpdateKind == PointerUpdateKind.XButton1Released,
            MouseButton.XButton2 => pointerPoint.PointerUpdateKind == PointerUpdateKind.XButton2Released,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static IDisposable BindTo<T>(this IObservable<T> source, AvaloniaObject target, AvaloniaProperty<T> property)
    {
        return source
            .DistinctUntilChanged()
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(value => target.SetValue(property, value));
    }
}