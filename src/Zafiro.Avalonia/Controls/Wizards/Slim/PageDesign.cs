using Zafiro.UI.Wizards.Slim;

namespace Zafiro.Avalonia.Controls.Wizards.Slim;

public class PageDesign : IPage
{
    public object Content { get; } = "This is some content";
    public string Title { get; } = "Title";
    public IObservable<string> TitleObservable { get; } = Observable.Return("Title");
    public int Index { get; } = 2;
    public StepKind Kind { get; } = StepKind.Normal;
}