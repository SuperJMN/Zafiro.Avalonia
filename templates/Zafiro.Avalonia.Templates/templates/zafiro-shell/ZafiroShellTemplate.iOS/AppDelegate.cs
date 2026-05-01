using Foundation;
using Avalonia;
using Avalonia.iOS;
using ReactiveUI.Avalonia;

namespace ZafiroShellTemplate.iOS;

[Register("AppDelegate")]
public partial class AppDelegate : AvaloniaAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI(_ => { });
    }
}
