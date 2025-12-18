using Avalonia.Controls.Primitives;
using CSharpFunctionalExtensions;
using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Controls.Wizards.Classic;

public class WizardHeader : TemplatedControl
{
    public static readonly StyledProperty<bool> IsBackButtonVisibleProperty = AvaloniaProperty.Register<WizardHeader, bool>(
        nameof(IsBackButtonVisible),
        defaultValue: true);

    public static readonly StyledProperty<Maybe<string>> TitleProperty = AvaloniaProperty.Register<WizardHeader, Maybe<string>>(
        nameof(Title));

    public static readonly StyledProperty<int> CurrentPageIndexProperty = AvaloniaProperty.Register<WizardHeader, int>(
        nameof(CurrentPageIndex));

    public static readonly StyledProperty<int> TotalPagesProperty = AvaloniaProperty.Register<WizardHeader, int>(
        nameof(TotalPages));

    public static readonly StyledProperty<IEnhancedCommand> BackProperty = AvaloniaProperty.Register<WizardHeader, IEnhancedCommand>(
        nameof(Back));

    public bool IsBackButtonVisible
    {
        get => GetValue(IsBackButtonVisibleProperty);
        set => SetValue(IsBackButtonVisibleProperty, value);
    }

    public Maybe<string> Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public int CurrentPageIndex
    {
        get => GetValue(CurrentPageIndexProperty);
        set => SetValue(CurrentPageIndexProperty, value);
    }

    public int TotalPages
    {
        get => GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public IEnhancedCommand Back
    {
        get => GetValue(BackProperty);
        set => SetValue(BackProperty, value);
    }
}