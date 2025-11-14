using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CSharpFunctionalExtensions;
using ReactiveUI;

namespace Zafiro.Avalonia.Controls;

public class TagSelector : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable<string>?> ItemsSourceProperty =
        AvaloniaProperty.Register<TagSelector, IEnumerable<string>?>(nameof(ItemsSource));

    public static readonly StyledProperty<int> MaxTagsProperty =
        AvaloniaProperty.Register<TagSelector, int>(nameof(MaxTags), 1);

    public static readonly StyledProperty<IDataTemplate?> TagTemplateProperty =
        AvaloniaProperty.Register<TagSelector, IDataTemplate?>(nameof(TagTemplate));

    public static readonly StyledProperty<StringComparison> StringComparisonProperty =
        AvaloniaProperty.Register<TagSelector, StringComparison>(nameof(StringComparison), StringComparison.OrdinalIgnoreCase);

    public IEnumerable<string>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public int MaxTags
    {
        get => GetValue(MaxTagsProperty);
        set => SetValue(MaxTagsProperty, Math.Max(1, value));
    }

    public IDataTemplate? TagTemplate
    {
        get => GetValue(TagTemplateProperty);
        set => SetValue(TagTemplateProperty, value);
    }

    public StringComparison StringComparison
    {
        get => GetValue(StringComparisonProperty);
        set => SetValue(StringComparisonProperty, value);
    }

    public ObservableCollection<string> SelectedTags { get; } = new();

    readonly ObservableCollection<string> suggestions = new();

    public ReactiveCommand<string, Unit> RemoveTagCommand { get; }

    TextBox? inputBox;
    ListBox? listBox;
    ListBox? suggestionsList;
    global::Avalonia.Controls.Primitives.Popup? popup;
    WrapPanel? panel;

    public TagSelector()
    {
        RemoveTagCommand = ReactiveCommand.Create<string>(RemoveTag);
        TagTemplateProperty.Changed.AddClassHandler<TagSelector>((x, _) => x.UpdateItemTemplate());
        AddHandler(PointerPressedEvent, (_, e) =>
        {
            if (!e.Handled)
            {
                inputBox?.Focus();
            }
        }, RoutingStrategies.Bubble);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        listBox = e.NameScope.Find<ListBox>("PART_SelectedItems");
        suggestionsList = e.NameScope.Find<ListBox>("PART_Suggestions");
        popup = e.NameScope.Find<global::Avalonia.Controls.Primitives.Popup>("PART_Popup");

        inputBox = new TextBox
        {
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            Background = Brushes.Transparent,
            MinWidth = 0
        };
        inputBox.GetObservable(TextBox.TextProperty).Subscribe(_ => UpdateSuggestions());
        inputBox.KeyDown += InputBoxOnKeyDown;

        if (listBox != null)
        {
            listBox.ItemsSource = SelectedTags;
            listBox.KeyDown += ListBoxOnKeyDown;
            listBox.AttachedToVisualTree += (_, _) => AttachInputBox();
            SelectedTags.CollectionChanged += (_, _) => AttachInputBox();
            panel = e.NameScope.Find<WrapPanel>("PART_Panel");
            UpdateItemTemplate();
        }

        if (suggestionsList != null)
        {
            suggestionsList.ItemsSource = suggestions;
            suggestionsList.DoubleTapped += (_, _) => AddSelectedSuggestion();
            suggestionsList.KeyDown += SuggestionsListOnKeyDown;
        }

        if (popup != null)
        {
            popup.PlacementTarget = inputBox;
        }

        AttachInputBox();
    }

    void UpdateItemTemplate()
    {
        if (listBox == null)
        {
            return;
        }

        listBox.ItemTemplate = new FuncDataTemplate<string>((item, _) => TagTemplate?.Build(item) ?? new TextBlock { Text = item });
    }

    void SuggestionsListOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddSelectedSuggestion();
            e.Handled = true;
        }
    }

    void ListBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && listBox?.SelectedItem is string toDelete)
        {
            RemoveTag(toDelete);
            e.Handled = true;
        }
        else if (e.Key == Key.Back)
        {
            if (listBox?.SelectedItem is string selected)
            {
                RemoveTag(selected);
                e.Handled = true;
            }
            else if (string.IsNullOrEmpty(inputBox?.Text))
            {
                RemoveLastTag();
                e.Handled = true;
            }
        }
    }

    void InputBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            var first = suggestions.FirstOrDefault();
            AddTag(first);
            e.Handled = true;
        }
        else if (e.Key == Key.Back && string.IsNullOrEmpty(inputBox?.Text))
        {
            RemoveLastTag();
            e.Handled = true;
        }
    }

    void AddSelectedSuggestion()
    {
        if (suggestionsList?.SelectedItem is string s)
        {
            AddTag(s);
        }
    }

    void UpdateSuggestions()
    {
        if (inputBox == null)
        {
            return;
        }

        suggestions.Clear();
        var text = inputBox.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            if (popup != null)
            {
                popup.IsOpen = false;
            }
            return;
        }

        var source = ItemsSource ?? Enumerable.Empty<string>();
        foreach (var tag in source)
        {
            if (tag.Contains(text, StringComparison) && !SelectedTags.Contains(tag, GetComparer()))
            {
                suggestions.Add(tag);
            }
        }

        if (popup != null)
        {
            popup.IsOpen = suggestions.Any();
        }
    }

    void AddTag(string? tag)
    {
        Result.Success(tag ?? string.Empty)
            .Ensure(t => !string.IsNullOrWhiteSpace(t), "Empty")
            .Ensure(t => !SelectedTags.Contains(t, GetComparer()), "Duplicate")
            .Ensure(_ => SelectedTags.Count < MaxTags, "MaxReached")
            .Tap(t =>
            {
                SelectedTags.Add(t);
            })
            .Tap(() =>
            {
                if (inputBox != null)
                {
                    inputBox.Text = string.Empty;
                    inputBox.Focus();
                }
                UpdateSuggestions();
                AttachInputBox();
            });
    }

    void RemoveTag(string tag)
    {
        SelectedTags.Remove(tag);
        UpdateSuggestions();
        AttachInputBox();
    }

    void RemoveLastTag()
    {
        if (SelectedTags.Count > 0)
        {
            SelectedTags.RemoveAt(SelectedTags.Count - 1);
            UpdateSuggestions();
            AttachInputBox();
        }
    }

    void AttachInputBox()
    {
        if (listBox == null || inputBox == null)
        {
            return;
        }

        panel ??= listBox.ItemsPanelRoot as WrapPanel;
        if (panel == null)
        {
            return;
        }

        if (!panel.Children.Contains(inputBox))
        {
            panel.Children.Add(inputBox);
        }
    }

    StringComparer GetComparer() => StringComparison switch
    {
        StringComparison.CurrentCulture => StringComparer.CurrentCulture,
        StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
        StringComparison.InvariantCulture => StringComparer.InvariantCulture,
        StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
        StringComparison.Ordinal => StringComparer.Ordinal,
        StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
        _ => StringComparer.Ordinal
    };
}

