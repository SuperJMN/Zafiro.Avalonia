using System.Globalization;
using System.Reactive.Disposables;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Misc;

namespace Zafiro.Avalonia.Behaviors;

public class NumberBoxBehavior : Behavior<TextBox>
{
    private static readonly string Separator = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
    private readonly CompositeDisposable disposables = new();


    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();

        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.OnEvent(InputElement.TextInputEvent, RoutingStrategies.Tunnel)
            .Select(x => x.EventArgs)
            .Do(x => InsertImplicitZero(x, AssociatedObject, x.Text ?? ""))
            .Do(x => Filter(x, AssociatedObject, x.Text))
            .Subscribe()
            .DisposeWith(disposables);

        AssociatedObject.OnEvent(TextBox.PastingFromClipboardEvent, RoutingStrategies.Bubble)
            .SelectMany(async x => (x.EventArgs, Text: await GetClipboardText()))
            .ObserveOn(AvaloniaScheduler.Instance)
            .Do(x => PrependZeroOnPaste(x.EventArgs, AssociatedObject, x.Text))
            .Do(pasting => Filter(pasting.EventArgs, AssociatedObject, pasting.Text))
            .Subscribe()
            .DisposeWith(disposables);
    }

    protected override void OnDetachedFromVisualTree()
    {
        disposables.Dispose();
    }

    private static Task<string> GetClipboardText()
    {
        return ApplicationUtils.GetClipboard()
            .Map(clipboard => clipboard.TryGetTextAsync())
            .GetValueOrDefault(_ => "")!;
    }

    private static void PrependZeroOnPaste(RoutedEventArgs pastingEventArgs, TextBox textBox, string toPaste)
    {
        if (textBox.Text is null)
        {
            return;
        }

        if (textBox.SelectedText == textBox.Text && toPaste.StartsWith(Separator))
        {
            pastingEventArgs.Handled = true;
            textBox.Text = "0" + toPaste;
            textBox.ClearSelection();
            textBox.CaretIndex = textBox.Text.Length;
            return;
        }

        if (textBox.Text.Contains(CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator) ||
            !toPaste.StartsWith('.'))
        {
            return;
        }

        if (textBox.CaretIndex != 0)
        {
            return;
        }

        pastingEventArgs.Handled = true;
        textBox.Text = "0" + toPaste;
    }

    private static void Filter(RoutedEventArgs arg, TextBox tb, string? newText)
    {
        arg.Handled = !IsValid(SimulateNextText(newText, tb), tb.Text);
    }


    private static bool IsValid(string str, string? currentText)
    {
        if (currentText == "" && str == CultureInfo.CurrentUICulture.NumberFormat.NegativeSign)
        {
            return true;
        }

        if (str.Any(char.IsWhiteSpace))
        {
            return false;
        }

        return decimal.TryParse(str, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentUICulture, out _);
    }

    private static string SimulateNextText(string? text, TextBox tb)
    {
        var start = Math.Min(tb.SelectionStart, tb.SelectionEnd);
        var end = Math.Max(tb.SelectionStart, tb.SelectionEnd);

        return tb.Text?[..start] + text + tb.Text?[end..];
    }

    private static void InsertImplicitZero(TextInputEventArgs textInputEventArgs, TextBox textBox, string text)
    {
        if (textBox.SelectedText == textBox.Text && text.StartsWith(Separator))
        {
            textInputEventArgs.Text = "0" + text;
            return;
        }

        if (textBox.Text == null)
        {
            return;
        }

        if (textBox.Text.Contains(Separator) || !text.StartsWith(Separator))
        {
            return;
        }

        if (textBox.CaretIndex != 0)
        {
            return;
        }

        var finalText = "0" + text;
        if (!decimal.TryParse(finalText, out _))
        {
            return;
        }

        textInputEventArgs.Text = finalText;
    }
}