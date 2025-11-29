using System.Reactive;
using CSharpFunctionalExtensions;
using Zafiro.UI.Commands;
using Zafiro.UI.Navigation;

namespace Zafiro.Avalonia.Controls.Shell;

public class SimpleNavigator : INavigator
{
    public SimpleNavigator(object content)
    {
        Content = Observable.Defer(() => Observable.Return(content));
    }

    public IObservable<object?> Content { get; }
    public IEnhancedCommand<Result> Back { get; }

    public Task<Result<Unit>> Go(Func<object> factory)
    {
        throw new NotSupportedException();
    }

    public Task<Result<Unit>> Go(Type type)
    {
        throw new NotSupportedException();
    }

    public Task<Result<Unit>> GoBack()
    {
        throw new NotSupportedException();
    }

    public NavigationBookmark CreateBookmark()
    {
        throw new NotSupportedException();
    }

    public void CreateBookmark(string name)
    {
        throw new NotSupportedException();
    }

    public Task<Result<Unit>> GoBackTo(NavigationBookmark bookmark)
    {
        throw new NotSupportedException();
    }

    public Task<Result<Unit>> GoBackTo(string name)
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {
    }
}