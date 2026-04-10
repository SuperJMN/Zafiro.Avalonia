using Avalonia.Xaml.Interactivity;

namespace Zafiro.Avalonia.Behaviors;

public class TypewriterBehavior : DisposingBehavior<TextBlock>
{
    public static readonly StyledProperty<string> TextToTypeProperty = AvaloniaProperty.Register<TypewriterBehavior, string>(
        nameof(TextToType));

    public string TextToType
    {
        get => GetValue(TextToTypeProperty);
        set => SetValue(TextToTypeProperty, value);
    }

    protected override IDisposable OnAttachedOverride()
    {
        return this.WhenAnyValue(x => x.TextToType)
            .WhereNotNull()
            .Throttle(TimeSpan.FromSeconds(1), RxSchedulers.MainThreadScheduler)
            .Select(s => Remove(AssociatedObject.Text).Concat(Add(TextToType)))
            .Switch()
            .Do(text => AssociatedObject.Text = text)
            .Subscribe();
    }

    private IObservable<string> Add(string text)
    {
        return Enumerable.Range(0, text.Length)
            .Reverse()
            .ToObservable()
            .Select(i => Observable.Return(text[..^i]).Delay(TimeSpan.FromMilliseconds(50), AvaloniaScheduler.Instance))
            .Concat();
    }

    private IObservable<string> Remove(string text)
    {
        return Enumerable.Range(0, text.Length)
            .ToObservable()
            .Select(i => Observable.Return(text[..^i]).Delay(TimeSpan.FromMilliseconds(50), AvaloniaScheduler.Instance))
            .Concat();
    }
}