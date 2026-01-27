using System.Reactive.Disposables;
using Avalonia.Xaml.Interactivity;
using Zafiro.Avalonia.Misc;
using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Controls.Navigation;

public class AutoHeaderFooterBehavior : Behavior<Frame>
{
    private CompositeDisposable? subscriptions;

    protected override void OnAttachedToVisualTree()
    {
        subscriptions?.Dispose();
        subscriptions = new CompositeDisposable();

        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.GetObservable(Frame.ContentProperty)
            .Select(content =>
            {
                if (content is IHaveHeader haveHeader)
                {
                    return haveHeader.Header.Select(x => (object?)x);
                }

                return Observable.Return<object?>(null);
            })
            .Switch()
            .BindTo(AssociatedObject, Frame.HeaderProperty)
            .DisposeWith(subscriptions);

        AssociatedObject.GetObservable(Frame.ContentProperty)
            .Select(content =>
            {
                if (content is IHaveFooter haveFooter)
                {
                    return haveFooter.Footer.Select(x => (object?)x);
                }

                return Observable.Return<object?>(null);
            })
            .Switch()
            .BindTo(AssociatedObject, Frame.FooterProperty)
            .DisposeWith(subscriptions);
    }

    protected override void OnDetachedFromVisualTree()
    {
        subscriptions?.Dispose();
        subscriptions = null;
        base.OnDetachedFromVisualTree();
    }
}