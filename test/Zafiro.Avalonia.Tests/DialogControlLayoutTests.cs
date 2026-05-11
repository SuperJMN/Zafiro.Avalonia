using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Zafiro.Avalonia.Controls;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.Dialogs.Views;

namespace Zafiro.Avalonia.Tests;

public class DialogControlLayoutTests
{
    private static readonly object StylesLock = new();

    [AvaloniaFact]
    public void Three_dialog_actions_are_compact_and_share_one_row()
    {
        EnsureDialogStyles();

        var dialog = new DialogControl
        {
            Title = "Installer",
            Content = new TextBlock { Text = "Install the application." },
            Options =
            [
                new OptionDesign { Title = "Cancel", Role = OptionRole.Cancel, IsCancel = true },
                new OptionDesign { Title = "Back", Role = OptionRole.Secondary },
                new OptionDesign { Title = "Next", Role = OptionRole.Primary, IsDefault = true },
            ],
        };

        ShowAndLayout(dialog, 520, 360);
        Assert.NotNull(dialog.Template);

        var buttons = dialog
            .GetVisualDescendants()
            .OfType<EnhancedButton>()
            .OrderBy(button => button.Bounds.X)
            .ToList();

        Assert.Equal(3, buttons.Count);
        Assert.Equal([OptionRole.Secondary, OptionRole.Primary, OptionRole.Cancel], buttons.Select(button => ((IOption)button.DataContext!).Role));
        Assert.Single(buttons.Select(button => Math.Round(button.Bounds.Y, 1)).Distinct());
        Assert.All(buttons, button => Assert.InRange(button.Bounds.Width, 80, 160));
        Assert.All(buttons, button => Assert.True(button.Bounds.Height <= 44));

        var buttonBounds = buttons.Select(button => BoundsRelativeTo(button, dialog)).ToList();
        var rightEdge = buttonBounds.Max(bounds => bounds.Right);
        var bottomEdge = buttonBounds.Max(bounds => bounds.Bottom);

        Assert.True(rightEdge <= dialog.Bounds.Width - 20 + 0.5);
        Assert.InRange(bottomEdge, dialog.Bounds.Height - 20 - 0.5, dialog.Bounds.Height - 20 + 0.5);
    }

    [AvaloniaFact]
    public void Dialog_action_text_keeps_its_intrinsic_width()
    {
        EnsureDialogStyles();

        var primary = new OptionDesign { Title = "Next", Role = OptionRole.Primary, IsDefault = true };
        var dialog = new DialogControl
        {
            Title = "Installer",
            Content = new TextBlock { Text = "Ready to install." },
            Options =
            [
                new OptionDesign { Title = "Back", Role = OptionRole.Secondary },
                primary,
                new OptionDesign { Title = "Cancel", Role = OptionRole.Cancel, IsCancel = true },
            ],
        };

        ShowAndLayout(dialog, 485, 368);
        primary.Title = "Install";
        Dispatcher.UIThread.RunJobs();

        var installButton = dialog
            .GetVisualDescendants()
            .OfType<EnhancedButton>()
            .Single(button => ((IOption)button.DataContext!).Role == OptionRole.Primary);

        var installText = Assert.Single(installButton.GetVisualDescendants().OfType<TextBlock>().Where(text => text is { Text: "Install", IsVisible: true } && text.Bounds.Width > 0));
        Assert.True(
            installText.Bounds.Width >= installText.DesiredSize.Width - 0.5,
            $"Install text was arranged at {installText.Bounds.Width}px but needs {installText.DesiredSize.Width}px.");

        Assert.True(
            installText.Bounds.Height <= installText.DesiredSize.Height + 0.5,
            $"Install text was stretched to {installText.Bounds.Height}px but only needs {installText.DesiredSize.Height}px.");

        var textBounds = BoundsRelativeTo(installText, installButton);
        Assert.InRange(textBounds.Center.X, installButton.Bounds.Width / 2 - 1, installButton.Bounds.Width / 2 + 1);
        Assert.InRange(textBounds.Center.Y, installButton.Bounds.Height / 2 - 1, installButton.Bounds.Height / 2 + 1);
    }

    private static Rect BoundsRelativeTo(Control control, Visual ancestor)
    {
        var origin = control.TranslatePoint(new Point(), ancestor);
        Assert.NotNull(origin);

        return new Rect(origin.Value, control.Bounds.Size);
    }

    private static void ShowAndLayout(Control control, double width, double height)
    {
        var window = new Window
        {
            Content = control,
            Width = width,
            Height = height,
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        control.ApplyTemplate();
        control.Measure(new Size(width, height));
        control.Arrange(new Rect(0, 0, width, height));
        Dispatcher.UIThread.RunJobs();
    }

    private static void EnsureDialogStyles()
    {
        lock (StylesLock)
        {
            var styles = Application.Current!.Styles;

            if (!styles.OfType<FluentTheme>().Any())
            {
                styles.Insert(0, new FluentTheme());
            }

            AddStyleIncludeOnce(styles, "avares://Zafiro.Avalonia/", "avares://Zafiro.Avalonia/Styles.axaml");
            AddStyleIncludeOnce(styles, "avares://Zafiro.Avalonia.Dialogs/", "avares://Zafiro.Avalonia.Dialogs/Styles.axaml");
        }
    }

    private static void AddStyleIncludeOnce(global::Avalonia.Styling.Styles styles, string baseUri, string source)
    {
        var sourceUri = new Uri(source);
        if (styles.OfType<StyleInclude>().Any(style => style.Source == sourceUri))
        {
            return;
        }

        styles.Add(new StyleInclude(new Uri(baseUri)) { Source = sourceUri });
    }
}
