using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;

namespace Zafiro.Avalonia.Misc;

[PublicAPI]
public static class ApplicationUtils
{
    public static IObservable<Thickness> SafeAreaPadding
    {
        get
        {
            var topLevel = TopLevel()
                .Bind(level => level.InsetsManager.AsMaybe())
                .Map(insetsManager =>
                {
                    return Observable.FromEventPattern<SafeAreaChangedArgs>(h => insetsManager.SafeAreaChanged += h, h => insetsManager.SafeAreaChanged -= h).Select(pattern => pattern.EventArgs.SafeAreaPadding)
                        .StartWith(insetsManager.SafeAreaPadding);
                });

            return topLevel.GetValueOrDefault(Observable.Empty<Thickness>());
        }
    }

    public static Maybe<IClipboard> GetClipboard()
    {
        return TopLevel().Bind(topLevel => topLevel.Clipboard.AsMaybe());
    }

    public static Maybe<TopLevel> TopLevel()
    {
        return CurrentContent().Bind(cv => global::Avalonia.Controls.TopLevel.GetTopLevel(cv).AsMaybe());
    }

    public static Maybe<Control> CurrentContent()
    {
        return MainWindow().Select(ContentControl (window) => window).Or(MainView().Select(visual => (ContentControl)visual)).Map(visual => (Control)visual.Content);
    }

    public static Maybe<AdornerLayer> CurrentAdornerLayer()
    {
        return CurrentContent().Map(arg => AdornerLayer.GetAdornerLayer(arg));
    }

    public static Maybe<Visual> MainView()
    {
        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            return singleView.MainView;
        }

        return Maybe<Visual>.None;
    }

    public static Maybe<Window> MainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow;
        }

        return Maybe<Window>.None;
    }

    public static void Connect(this Application application,
        Func<Control> createMainView,
        Func<Control, object> createDataContext,
        Func<Window>? createApplicationWindow = default)
    {
        var mainView = createMainView();
        StyledElement dataContextTarget = mainView;
        switch (application.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                var window = createApplicationWindow?.Invoke() ?? new Window();
                window.Content = mainView;
                desktop.MainWindow = window;
                dataContextTarget = window;
                break;
            }
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = mainView;
                break;
        }

        dataContextTarget.DataContext = createDataContext(mainView);
    }

    public static Task<T> ExecuteOnUIThreadAsync<T>(this Func<Task<T>> func)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return func();
        }

        return Dispatcher.UIThread.InvokeAsync(func);
    }

    public static T ExecuteOnUIThread<T>(this Func<T> func)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return func();
        }

        return Dispatcher.UIThread.Invoke(func);
    }

    public static void ExecuteOnUIThread(this Action func)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            func();
        }
        else
        {
            Dispatcher.UIThread.Invoke(func);
        }
    }
}