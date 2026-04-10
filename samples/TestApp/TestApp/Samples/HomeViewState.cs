using System.Reactive;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Styling;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace TestApp.Samples;

public partial class HomeViewState : ReactiveObject
{
    [Reactive] private bool isDarkTheme;
    [Reactive] private string? searchText;
    [Reactive] private string? selectedCategory = "All";

    public HomeViewState()
    {
        // Initialize based on the current system theme if possible, or default to false (Light).
        if (Application.Current?.PlatformSettings?.GetColorValues().ThemeVariant == PlatformThemeVariant.Dark)
        {
            isDarkTheme = true;
        }

        ToggleTheme = ReactiveCommand.Create(() =>
        {
            IsDarkTheme = !IsDarkTheme;
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeVariant = IsDarkTheme ? ThemeVariant.Dark : ThemeVariant.Light;
            }
        });
    }

    public ReactiveCommand<Unit, Unit> ToggleTheme { get; }
}