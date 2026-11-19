using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Controls;
using Zafiro.Avalonia.Controls.Navigation;
using Zafiro.Avalonia.Controls.Shell;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.Dialogs.Implementations;

namespace Zafiro.Avalonia.Tests;

/// <summary>
/// Keyboard, TV remote and gamepad users only have focus to know where they are, so it must never get lost or leak
/// behind a dialog.
/// </summary>
public class KeyboardNavigationTests
{
    private static readonly object StylesLock = new();

    [AvaloniaFact]
    public void Arrow_key_with_nothing_focused_starts_on_the_page_instead_of_the_controls_around_it()
    {
        var pageButton = new Button { Content = "Page" };
        var statusBar = new Button { Content = "Status bar" };
        var window = Show(new DockPanel
        {
            Children =
            {
                Dock(statusBar, global::Avalonia.Controls.Dock.Bottom),
                new Frame { Content = new StackPanel { Children = { pageButton } } },
            },
        });

        Press(window, Key.Down);

        Assert.Same(pageButton, FocusedElement(window));
    }

    [AvaloniaFact]
    public void Navigating_from_the_keyboard_moves_focus_into_the_new_page()
    {
        var open = new Button { Content = "Open" };
        var details = new Button { Content = "Details action" };
        var frame = new Frame { Content = new StackPanel { Children = { open } } };
        var window = Show(frame);
        Press(window, Key.Tab);
        Assert.Same(open, FocusedElement(window));

        frame.Content = new StackPanel { Children = { details } };
        Dispatcher.UIThread.RunJobs();

        Assert.Same(details, FocusedElement(window));
    }

    [AvaloniaFact]
    public void Going_back_from_the_keyboard_returns_focus_to_the_item_that_was_used()
    {
        var frame = ListFrame(Items("One", "Two", "Three"));
        var window = Show(frame);
        Press(window, Key.Tab);
        Press(window, Key.Down);
        Assert.Equal("Two", FocusedText(window));

        frame.Content = new DetailsViewModel();
        Dispatcher.UIThread.RunJobs();
        // Navigators build the previous page again when going back.
        frame.Content = new ListViewModel(Items("One", "Two", "Three"));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Two", FocusedText(window));
    }

    [AvaloniaFact]
    public void Going_back_to_a_page_that_loads_its_items_later_returns_focus_to_the_item_once_it_shows_up()
    {
        var frame = ListFrame(Items("One", "Two", "Three"));
        var window = Show(frame);
        Press(window, Key.Tab);
        Press(window, Key.Down);
        frame.Content = new DetailsViewModel();
        Dispatcher.UIThread.RunJobs();

        var reloaded = new ListViewModel([]);
        frame.Content = reloaded;
        Dispatcher.UIThread.RunJobs();
        foreach (var item in Items("One", "Two", "Three"))
        {
            reloaded.Items.Add(item);
        }

        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Two", FocusedText(window));
    }

    [AvaloniaFact]
    public void Focus_entering_a_page_on_its_own_skips_text_boxes_so_the_on_screen_keyboard_stays_closed()
    {
        var search = new TextBox();
        var firstItem = new Button { Content = "Item" };
        var window = Show(new Frame { Content = new StackPanel { Children = { search, firstItem } } });

        Press(window, Key.Down);

        Assert.Same(firstItem, FocusedElement(window));
    }

    [AvaloniaFact]
    public void Navigating_by_touch_does_not_move_focus()
    {
        var open = new Button { Content = "Open" };
        var details = new TextBox();
        var frame = new Frame { Content = new StackPanel { Children = { open } } };
        var window = Show(frame);
        Tap(window, open);

        frame.Content = new StackPanel { Children = { details } };
        Dispatcher.UIThread.RunJobs();

        Assert.False(details.IsFocused);
    }

    [AvaloniaFact]
    public void Releasing_space_after_it_opened_a_page_does_not_press_the_button_that_got_focus()
    {
        var clicks = 0;
        var action = new Button { Content = "Queue build" };
        action.Click += (_, _) => clicks++;
        var item = new Button { Content = "Project" };
        var frame = new Frame { Content = new StackPanel { Children = { item } } };
        // Like a list row, the item opens its page as soon as the key goes down.
        item.AddHandler(InputElement.KeyDownEvent, (_, e) =>
        {
            if (e.Key == Key.Space)
            {
                frame.Content = new StackPanel { Children = { action } };
            }
        }, handledEventsToo: true);
        var window = Show(frame);
        Press(window, Key.Tab);
        Assert.Same(item, FocusedElement(window));

        window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
        Dispatcher.UIThread.RunJobs();
        Assert.Same(action, FocusedElement(window));
        window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, clicks);
    }

    [AvaloniaFact]
    public void Space_still_presses_the_focused_button()
    {
        var clicks = 0;
        var button = new Button { Content = "Save" };
        button.Click += (_, _) => clicks++;
        var window = Show(new StackPanel { Children = { button } });
        Press(window, Key.Tab);

        Press(window, Key.Space);

        Assert.Equal(1, clicks);
    }

    [AvaloniaFact]
    public void Section_strip_items_can_be_reached_from_the_keyboard()
    {
        var page = new Button { Content = "Page" };
        var strip = new SectionStrip
        {
            Orientation = Orientation.Horizontal,
            Sections = [Section("Home"), Section("Jobs"), Section("Settings")],
        };
        var window = Show(new StackPanel { Children = { page, strip } });
        page.Focus(NavigationMethod.Tab);

        Press(window, Key.Tab);

        Assert.IsType<SectionStripItem>(FocusedElement(window));
    }

    [AvaloniaFact]
    public void Arrowing_into_the_section_strip_lands_on_the_selected_section()
    {
        var sections = new[] { Section("Home"), Section("Jobs"), Section("Settings") };
        var page = new Button { Content = "Page", HorizontalAlignment = HorizontalAlignment.Left };
        var strip = new SectionStrip
        {
            Orientation = Orientation.Horizontal,
            Sections = sections,
            SelectedSection = sections[2],
        };
        var window = Show(new StackPanel { Children = { page, strip } });
        page.Focus(NavigationMethod.Directional);

        Press(window, Key.Down);

        var focused = Assert.IsType<SectionStripItem>(FocusedElement(window));
        Assert.Same(sections[2], focused.DataContext);
    }

    [AvaloniaFact]
    public void Dialog_opened_from_the_keyboard_takes_focus_and_gives_it_back_when_closed()
    {
        var opener = new Button { Content = "Open dialog" };
        var window = Show(new StackPanel { Children = { opener } });
        var dialogs = new AdornerDialog(() => AdornerLayer.GetAdornerLayer(opener)!);
        Press(window, Key.Tab);
        var field = new TextBox();

        _ = dialogs.Show(Maybe<Control>.From(field), Maybe<IObservable<string>>.From(Observable.Return("Dialog")), (_, closeable) => Options(closeable));
        Dispatcher.UIThread.RunJobs();
        Assert.Same(field, FocusedElement(window));

        dialogs.Dismiss();
        Dispatcher.UIThread.RunJobs();
        Assert.Same(opener, FocusedElement(window));
    }

    [AvaloniaFact]
    public void Keyboard_focus_cannot_leave_an_open_dialog()
    {
        var behind = new Button { Content = "Behind" };
        var window = Show(new StackPanel { Children = { behind, new Button { Content = "Also behind" } } });
        var dialogs = new AdornerDialog(() => AdornerLayer.GetAdornerLayer(behind)!);
        var field = new TextBox();
        _ = dialogs.Show(Maybe<Control>.From(field), Maybe<IObservable<string>>.From(Observable.Return("Dialog")), (_, closeable) => Options(closeable));
        Dispatcher.UIThread.RunJobs();
        var dialog = window.GetVisualDescendants().OfType<Dialogs.Views.DialogViewContainer>().Single();

        var visited = Enumerable.Range(0, 8).Select(_ =>
        {
            Press(window, Key.Tab);
            return FocusedElement(window);
        }).ToList();

        Assert.All(visited, element => Assert.True(dialog.IsVisualAncestorOf((Visual)element!), $"{element} is outside the dialog"));
    }

    [AvaloniaFact]
    public void Arrow_keys_move_between_the_dialog_fields_even_with_controls_behind_it()
    {
        var behind = new StackPanel();
        foreach (var index in Enumerable.Range(0, 20))
        {
            behind.Children.Add(new Button { Content = $"Behind {index}", Height = 20, HorizontalAlignment = HorizontalAlignment.Stretch });
        }

        var window = Show(behind);
        var dialogs = new AdornerDialog(() => AdornerLayer.GetAdornerLayer(behind)!);
        Press(window, Key.Tab);
        var first = new TextBox();
        var second = new TextBox { Margin = new Thickness(0, 40, 0, 0) };
        _ = dialogs.Show(Maybe<Control>.From(new StackPanel { Children = { first, second } }), Maybe<IObservable<string>>.From(Observable.Return("Dialog")), (_, closeable) => Options(closeable));
        Dispatcher.UIThread.RunJobs();
        Assert.Same(first, FocusedElement(window));

        Press(window, Key.Down);

        Assert.Same(second, FocusedElement(window));
    }

    [AvaloniaFact]
    public void Escape_dismisses_the_dialog()
    {
        var opener = new Button { Content = "Open dialog" };
        var window = Show(new StackPanel { Children = { opener } });
        var dialogs = new AdornerDialog(() => AdornerLayer.GetAdornerLayer(opener)!);
        Press(window, Key.Tab);
        var shown = dialogs.Show(Maybe<Control>.From(new TextBox()), Maybe<IObservable<string>>.From(Observable.Return("Dialog")), (_, closeable) => Options(closeable));
        Dispatcher.UIThread.RunJobs();

        Press(window, Key.Escape);

        Assert.True(shown.IsCompletedSuccessfully);
        Assert.False(shown.Result);
        Assert.Same(opener, FocusedElement(window));
    }

    [AvaloniaFact]
    public void System_back_dismisses_the_dialog_instead_of_reaching_the_page_behind_it()
    {
        var backRequestsSeenByPage = 0;
        var opener = new Button { Content = "Open dialog" };
        var window = Show(new StackPanel { Children = { opener } });
        window.AddHandler(TopLevel.BackRequestedEvent, (_, _) => backRequestsSeenByPage++);
        var dialogs = new AdornerDialog(() => AdornerLayer.GetAdornerLayer(opener)!);
        var shown = dialogs.Show(Maybe<Control>.From(new TextBox()), Maybe<IObservable<string>>.From(Observable.Return("Dialog")), (_, closeable) => Options(closeable));
        Dispatcher.UIThread.RunJobs();

        window.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(TopLevel.BackRequestedEvent));
        Dispatcher.UIThread.RunJobs();

        Assert.True(shown.IsCompletedSuccessfully);
        Assert.Equal(0, backRequestsSeenByPage);
    }

    [AvaloniaFact]
    public void Dialog_taller_than_the_window_keeps_its_actions_on_screen()
    {
        var opener = new Button { Content = "Open dialog" };
        var window = Show(new StackPanel { Children = { opener } });
        var dialogs = new AdornerDialog(() => AdornerLayer.GetAdornerLayer(opener)!);
        var tallForm = new StackPanel { Children = { new TextBox(), new Border { Height = 2000 }, new TextBox() } };
        _ = dialogs.Show(Maybe<Control>.From(tallForm), Maybe<IObservable<string>>.From(Observable.Return("Dialog")), (_, closeable) => Options(closeable));
        Dispatcher.UIThread.RunJobs();

        var action = window.GetVisualDescendants().OfType<Button>().Single(button => button.DataContext is OptionDesign);
        var bottom = action.TranslatePoint(new Point(0, action.Bounds.Height), window)!.Value.Y;

        Assert.InRange(bottom, 0, window.Bounds.Height);
    }

    [AvaloniaFact]
    public void Tab_goes_through_the_dialog_content_before_its_actions()
    {
        var opener = new Button { Content = "Open dialog" };
        var window = Show(new StackPanel { Children = { opener } });
        var dialogs = new AdornerDialog(() => AdornerLayer.GetAdornerLayer(opener)!);
        Press(window, Key.Tab);
        var first = new TextBox();
        var second = new TextBox();
        _ = dialogs.Show(Maybe<Control>.From(new StackPanel { Children = { first, second } }), Maybe<IObservable<string>>.From(Observable.Return("Dialog")), (_, closeable) => Options(closeable));
        Dispatcher.UIThread.RunJobs();

        Press(window, Key.Tab);
        Assert.Same(second, FocusedElement(window));
        Press(window, Key.Tab);
        Assert.IsType<OptionDesign>(((Control)FocusedElement(window)!).DataContext);
    }

    private static IEnumerable<IOption> Options(Dialogs.ICloseable closeable)
    {
        return [new OptionDesign { Title = "OK", Role = OptionRole.Primary, IsDefault = true }];
    }

    private static SimpleSection Section(string name) => new() { Id = name, FriendlyName = name, Icon = name };

    private static Control Dock(Control control, Dock dock)
    {
        DockPanel.SetDock(control, dock);
        return control;
    }

    private static void Tap(Window window, Control control)
    {
        var center = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;
        window.MouseDown(center, MouseButton.Left);
        window.MouseUp(center, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static IInputElement? FocusedElement(TopLevel window) => window.FocusManager?.GetFocusedElement();

    private static void Press(Window window, Key key)
    {
        window.KeyPress(key, RawInputModifiers.None, PhysicalKey.None, null);
        window.KeyRelease(key, RawInputModifiers.None, PhysicalKey.None, null);
        Dispatcher.UIThread.RunJobs();
    }

    private static Window Show(Control content)
    {
        EnsureStyles();
        var window = new Window { Content = content, Width = 600, Height = 400 };
        // Arrow keys move focus between controls, as TV remotes and gamepads do.
        XYFocus.SetNavigationModes(window, XYFocusNavigationModes.Enabled);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void EnsureStyles()
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

    private sealed class Item(string name)
    {
        public string Name { get; } = name;
    }

    private static List<Item> Items(params string[] names) => names.Select(name => new Item(name)).ToList();

    private static Frame ListFrame(IEnumerable<Item> items)
    {
        var frame = new Frame { Content = new ListViewModel(items) };
        frame.DataTemplates.Add(new FuncDataTemplate<ListViewModel>((viewModel, _) => new ListPage(viewModel)));
        frame.DataTemplates.Add(new FuncDataTemplate<DetailsViewModel>((_, _) => new StackPanel { Children = { new Button { Content = "Details" } } }));
        return frame;
    }

    private static string? FocusedText(TopLevel window) => (FocusedElement(window) as ContentControl)?.Content?.ToString();

    private sealed class ListViewModel(IEnumerable<Item> items)
    {
        public System.Collections.ObjectModel.ObservableCollection<Item> Items { get; } = new(items);
    }

    private sealed class DetailsViewModel;

    // Built by a data template, so every visit shows a new view.
    private sealed class ListPage : ItemsControl
    {
        public ListPage(ListViewModel viewModel)
        {
            ItemsSource = viewModel.Items;
            ItemTemplate = new FuncDataTemplate<Item>((item, _) => new Button { Content = item.Name });
        }

        protected override Type StyleKeyOverride => typeof(ItemsControl);
    }
}
