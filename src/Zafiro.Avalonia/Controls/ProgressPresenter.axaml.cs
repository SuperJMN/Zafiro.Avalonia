using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.Primitives;
using Zafiro.Progress;

namespace Zafiro.Avalonia.Controls;

public class ProgressPresenter : TemplatedControl
{
    public static readonly StyledProperty<IProgress?> ProgressProperty =
        AvaloniaProperty.Register<ProgressPresenter, IProgress?>(nameof(Progress));

    public static readonly DirectProperty<ProgressPresenter, double> ProgressRatioProperty =
        AvaloniaProperty.RegisterDirect<ProgressPresenter, double>(nameof(ProgressRatio), presenter => presenter.ProgressRatio);

    public static readonly StyledProperty<string> NotStartedLabelProperty =
        AvaloniaProperty.Register<ProgressPresenter, string>(nameof(NotStartedLabel), "Not started");

    public static readonly StyledProperty<string> UnknownLabelProperty =
        AvaloniaProperty.Register<ProgressPresenter, string>(nameof(UnknownLabel), "In progress");

    public static readonly StyledProperty<string> CompletedLabelProperty =
        AvaloniaProperty.Register<ProgressPresenter, string>(nameof(CompletedLabel), "Completed");

    public static readonly StyledProperty<string> PercentageLabelProperty =
        AvaloniaProperty.Register<ProgressPresenter, string>(nameof(PercentageLabel), "Progress");

    public static readonly StyledProperty<string> CurrentLabelProperty =
        AvaloniaProperty.Register<ProgressPresenter, string>(nameof(CurrentLabel), "Current");

    public static readonly StyledProperty<string> TotalLabelProperty =
        AvaloniaProperty.Register<ProgressPresenter, string>(nameof(TotalLabel), "Total");

    public static readonly StyledProperty<string> UnitLabelProperty =
        AvaloniaProperty.Register<ProgressPresenter, string>(nameof(UnitLabel), string.Empty);

    private static readonly Type progressWithCurrentAndTotalType = typeof(ProgressWithCurrentAndTotal<>);

    private double progressRatio;

    public IProgress? Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public double ProgressRatio
    {
        get => progressRatio;
        private set => SetAndRaise(ProgressRatioProperty, ref progressRatio, value);
    }

    public string NotStartedLabel
    {
        get => GetValue(NotStartedLabelProperty);
        set => SetValue(NotStartedLabelProperty, value);
    }

    public string UnknownLabel
    {
        get => GetValue(UnknownLabelProperty);
        set => SetValue(UnknownLabelProperty, value);
    }

    public string CompletedLabel
    {
        get => GetValue(CompletedLabelProperty);
        set => SetValue(CompletedLabelProperty, value);
    }

    public string PercentageLabel
    {
        get => GetValue(PercentageLabelProperty);
        set => SetValue(PercentageLabelProperty, value);
    }

    public string CurrentLabel
    {
        get => GetValue(CurrentLabelProperty);
        set => SetValue(CurrentLabelProperty, value);
    }

    public string TotalLabel
    {
        get => GetValue(TotalLabelProperty);
        set => SetValue(TotalLabelProperty, value);
    }

    public string UnitLabel
    {
        get => GetValue(UnitLabelProperty);
        set => SetValue(UnitLabelProperty, value);
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ProgressProperty)
        {
            UpdateProgressState(change.GetNewValue<IProgress?>());
        }
    }

    private void UpdateProgressState(IProgress? progress)
    {
        if (progress is null)
        {
            ProgressRatio = 0;
            return;
        }

        if (progress is ProportionalProgress proportional)
        {
            ProgressRatio = ClampRatio(proportional.Ratio);
            return;
        }

        if (progress is Completed)
        {
            ProgressRatio = 1;
            return;
        }

        if (progress is NotStarted or Unknown)
        {
            ProgressRatio = 0;
            return;
        }

        if (TryComputeAbsoluteRatio(progress, out var ratio))
        {
            ProgressRatio = ratio;
            return;
        }

        ProgressRatio = 0;
    }

    private bool TryComputeAbsoluteRatio(IProgress progress, out double ratio)
    {
        var type = progress.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == progressWithCurrentAndTotalType)
        {
            var currentProperty = type.GetProperty(nameof(ProgressWithCurrentAndTotal<int>.Current));
            var totalProperty = type.GetProperty(nameof(ProgressWithCurrentAndTotal<int>.Total));

            var current = currentProperty?.GetValue(progress);
            var total = totalProperty?.GetValue(progress);

            ratio = CalculateRatio(current, total);
            return true;
        }

        ratio = 0;
        return false;
    }

    private static double CalculateRatio(object? current, object? total)
    {
        var currentValue = TryToDouble(current);
        var totalValue = TryToDouble(total);

        if (currentValue is null || totalValue is null || Math.Abs(totalValue.Value) < double.Epsilon)
        {
            return 0;
        }

        var ratio = currentValue.Value / totalValue.Value;
        return double.IsFinite(ratio) ? ClampRatio(ratio) : 0;
    }

    private static double? TryToDouble(object? value)
    {
        if (value is null)
        {
            return null;
        }

        try
        {
            return Convert.ToDouble(value, CultureInfo.CurrentCulture);
        }
        catch
        {
            return null;
        }
    }

    private static double ClampRatio(double value)
    {
        return Math.Clamp(value, 0, 1);
    }
}
