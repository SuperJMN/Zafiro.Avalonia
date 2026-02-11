using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Zafiro.Avalonia.Misc;

public class ClassAssist
{
    public static readonly AttachedProperty<string?> ClassesProperty =
        AvaloniaProperty.RegisterAttached<ClassAssist, StyledElement, string?>("Classes");

    public static readonly AttachedProperty<string?> ChildClassesProperty =
        AvaloniaProperty.RegisterAttached<ClassAssist, Control, string?>("ChildClasses");

    private static readonly AttachedProperty<IDisposable?> SubscriptionProperty =
        AvaloniaProperty.RegisterAttached<ClassAssist, Control, IDisposable?>("Subscription");

    static ClassAssist()
    {
        ClassesProperty.Changed.Subscribe(OnClassesChanged);
        ChildClassesProperty.Changed.Subscribe(OnChildClassesChanged);
    }

    public static void SetClasses(StyledElement element, string? value) => element.SetValue(ClassesProperty, value);
    public static string? GetClasses(StyledElement element) => element.GetValue(ClassesProperty);
    public static void SetChildClasses(Control element, string? value) => element.SetValue(ChildClassesProperty, value);
    public static string? GetChildClasses(Control element) => element.GetValue(ChildClassesProperty);

    private static void OnChildClassesChanged(AvaloniaPropertyChangedEventArgs<string?> e)
    {
        if (e.Sender is not Control control) return;

        CleanupSubscription(control);

        var classes = e.NewValue.GetValueOrDefault();
        if (string.IsNullOrEmpty(classes)) return;

        var subscription = ObservePresentedChild(control)
            .Select(child => child as StyledElement)
            .Scan(
                (Previous: (StyledElement?)null, Current: (StyledElement?)null),
                (acc, child) => (acc.Current, child))
            .Subscribe(pair =>
            {
                if (pair.Previous is not null) SetClasses(pair.Previous, null);
                if (pair.Current is not null) SetClasses(pair.Current, classes);
            });

        control.SetValue(SubscriptionProperty, subscription);
    }

    private static IObservable<Control?> ObservePresentedChild(Control control) => control switch
    {
        ContentPresenter cp => cp.GetObservable(ContentPresenter.ChildProperty),
        ContentControl cc => ObserveContentControlChild(cc),
        _ => Observable.Empty<Control?>()
    };

    private static IObservable<Control?> ObserveContentControlChild(ContentControl cc) =>
        Observable.FromEventPattern<TemplateAppliedEventArgs>(
                h => cc.TemplateApplied += h,
                h => cc.TemplateApplied -= h)
            .Select(_ => 0)
            .StartWith(0)
            .Select(_ => cc.GetVisualDescendants().OfType<ContentPresenter>().FirstOrDefault())
            .Where(p => p != null)
            .Select(p => p!.GetObservable(ContentPresenter.ChildProperty))
            .Switch();

    private static void CleanupSubscription(Control control)
    {
        var presenter = control is ContentPresenter cp
            ? cp
            : control.GetVisualDescendants().OfType<ContentPresenter>().FirstOrDefault();

        if (presenter?.Child is StyledElement child)
            SetClasses(child, null);

        control.GetValue(SubscriptionProperty)?.Dispose();
        control.SetValue(SubscriptionProperty, null);
    }

    private static void OnClassesChanged(AvaloniaPropertyChangedEventArgs<string?> e)
    {
        if (e.Sender is not StyledElement element) return;

        RemoveClasses(element, e.OldValue.GetValueOrDefault());
        AddClasses(element, e.NewValue.GetValueOrDefault());
    }

    private static void AddClasses(StyledElement element, string? classes)
    {
        if (classes is null) return;
        foreach (var c in classes.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            element.Classes.Add(c);
    }

    private static void RemoveClasses(StyledElement element, string? classes)
    {
        if (classes is null) return;
        foreach (var c in classes.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            element.Classes.Remove(c);
    }
}