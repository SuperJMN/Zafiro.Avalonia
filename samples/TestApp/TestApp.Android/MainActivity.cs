using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace TestApp.Android;

[Activity(
    Label = "Zafiro.Avalonia",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/Icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}