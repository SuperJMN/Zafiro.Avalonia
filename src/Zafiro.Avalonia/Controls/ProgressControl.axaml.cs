using Avalonia.Controls.Primitives;
using Zafiro.ProgressReporting;

namespace Zafiro.Avalonia.Controls;

public class ProgressControl : TemplatedControl
{
    public static readonly StyledProperty<Progress> ProgressProperty = AvaloniaProperty.Register<ProgressControl, Progress>(
        nameof(Progress));

    public static readonly StyledProperty<object> CompletedContentProperty = AvaloniaProperty.Register<ProgressControl, object>(
        nameof(CompletedContent));

    public static readonly StyledProperty<object> NotStartedContentProperty = AvaloniaProperty.Register<ProgressControl, object>(
        nameof(NotStartedContent));

    public static readonly StyledProperty<object> UnknownContentProperty = AvaloniaProperty.Register<ProgressControl, object>(
        nameof(UnknownContent));

    public Progress Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public object CompletedContent
    {
        get => GetValue(CompletedContentProperty);
        set => SetValue(CompletedContentProperty, value);
    }

    public object NotStartedContent
    {
        get => GetValue(NotStartedContentProperty);
        set => SetValue(NotStartedContentProperty, value);
    }

    public object UnknownContent
    {
        get => GetValue(UnknownContentProperty);
        set => SetValue(UnknownContentProperty, value);
    }
}