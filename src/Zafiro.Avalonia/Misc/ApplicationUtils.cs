using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
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
        return CurrentView().Bind(cv => global::Avalonia.Controls.TopLevel.GetTopLevel(cv).AsMaybe());
    }

    public static Maybe<Window> MainWindow()
    {
        return CurrentView().Bind(visual => (visual as Window).AsMaybe());
    }

    public static Maybe<Visual> CurrentView()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow;
        }

        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            return singleView.MainView;
        }

        throw new NotSupportedException("Unsupported application lifetime type.");
    }

    public static void Connect(this Application application,
        Func<Control> createMainView,
        Func<Control, object> createDataContext,
        Func<Window>? createApplicationWindow = default)
    {
        ConnectImpl(application, createMainView, control => Task.FromResult(createDataContext(control)), createApplicationWindow);
    }

    public static void Connect(this Application application,
        Func<Control> createMainView,
        Func<Control, Task<object>> createDataContext,
        Func<Window>? createApplicationWindow = default)
        => ConnectImpl(application, createMainView, createDataContext, createApplicationWindow);

    private static void ConnectImpl(Application application,
        Func<Control> createMainView,
        Func<Control, Task<object>> createDataContext,
        Func<Window>? createApplicationWindow)
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

        var dataContextTask = createDataContext(mainView);
        if (dataContextTask.IsCompleted)
        {
            dataContextTarget.DataContext = dataContextTask.GetAwaiter().GetResult();
            return;
        }

        _ = AssignDataContext(dataContextTask, dataContextTarget);

        static Task AssignDataContext(Task<object> source, StyledElement target)
        {
            async Task Run()
            {
                var dataContext = await source.ConfigureAwait(false);
                await Dispatcher.UIThread.InvokeAsync(() => target.DataContext = dataContext);
            }

            return Run();
        }
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

#if DEBUG
    public static bool IsDebug => true;
#else
    public static bool IsDebug => false;
#endif
}