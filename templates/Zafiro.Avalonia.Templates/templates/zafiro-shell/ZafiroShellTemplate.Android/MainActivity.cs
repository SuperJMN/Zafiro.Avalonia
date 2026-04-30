using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace ZafiroShellTemplate.Android;

[Activity(
    Label = "ZafiroShellTemplate",
    Theme = "@style/MyTheme.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
