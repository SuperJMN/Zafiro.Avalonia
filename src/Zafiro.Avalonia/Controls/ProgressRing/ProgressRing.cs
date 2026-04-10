// Based on FluentAvalonia ProgressRing by amwx
// Original source: https://github.com/amwx/FluentAvalonia
// License: MIT — Copyright (c) 2025 amwx

using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Zafiro.Avalonia.Controls;

[TemplatePart(TemplatePartAnimatedVisual, typeof(ProgressRingAnimatedVisual))]
public class ProgressRing : RangeBase
{
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsActive), true);

    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        ProgressBar.IsIndeterminateProperty.AddOwner<ProgressRing>(
            new StyledPropertyMetadata<bool>(defaultValue: true));

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _animatedVisualSource = e.NameScope.Get<ProgressRingAnimatedVisual>(TemplatePartAnimatedVisual);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
            _animatedVisualSource?.SetValue(change.GetNewValue<double>());
        else if (change.Property == MinimumProperty)
            _animatedVisualSource?.SetMinimum(change.GetNewValue<double>());
        else if (change.Property == MaximumProperty)
            _animatedVisualSource?.SetMaximum(change.GetNewValue<double>());
        else if (change.Property == IsIndeterminateProperty)
            _animatedVisualSource?.SetIndeterminate(change.GetNewValue<bool>());
        else if (change.Property == IsActiveProperty)
            _animatedVisualSource?.SetActive(change.GetNewValue<bool>());
        else if (change.Property == ForegroundProperty)
            _animatedVisualSource?.SetForeground((IBrush)change.NewValue!);
        else if (change.Property == BackgroundProperty)
            _animatedVisualSource?.SetBackground((IBrush)change.NewValue!);
    }

    private ProgressRingAnimatedVisual? _animatedVisualSource;
    private const string TemplatePartAnimatedVisual = "AnimatedVisual";
}
