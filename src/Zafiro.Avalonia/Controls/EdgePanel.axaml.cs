using Avalonia.Controls.Templates;

namespace Zafiro.Avalonia.Controls;

public class EdgePanel : ContentControl
{
    public static readonly StyledProperty<object> StartContentProperty = AvaloniaProperty.Register<EdgePanel, object>(
        nameof(StartContent));

    public static readonly StyledProperty<object> EndContentProperty = AvaloniaProperty.Register<EdgePanel, object>(
        nameof(EndContent));

    public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<EdgePanel, double>(
        nameof(Spacing));

    public static readonly StyledProperty<IDataTemplate> StartContentTemplateProperty = AvaloniaProperty.Register<EdgePanel, IDataTemplate>(
        nameof(StartContentTemplate));

    public static readonly StyledProperty<IDataTemplate> EndContentTemplateProperty = AvaloniaProperty.Register<EdgePanel, IDataTemplate>(
        nameof(EndContentTemplate));

    public static readonly StyledProperty<string> StartContentClassesProperty = AvaloniaProperty.Register<EdgePanel, string>(
        nameof(StartContentClasses));

    public static readonly StyledProperty<string> ContentClassesProperty = AvaloniaProperty.Register<EdgePanel, string>(
        nameof(ContentClasses));

    public static readonly StyledProperty<string> EndContentClassesProperty = AvaloniaProperty.Register<EdgePanel, string>(
        nameof(EndContentClasses));

    public object StartContent
    {
        get => GetValue(StartContentProperty);
        set => SetValue(StartContentProperty, value);
    }

    public object EndContent
    {
        get => GetValue(EndContentProperty);
        set => SetValue(EndContentProperty, value);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public IDataTemplate StartContentTemplate
    {
        get => GetValue(StartContentTemplateProperty);
        set => SetValue(StartContentTemplateProperty, value);
    }

    public IDataTemplate EndContentTemplate
    {
        get => GetValue(EndContentTemplateProperty);
        set => SetValue(EndContentTemplateProperty, value);
    }

    public string StartContentClasses
    {
        get => GetValue(StartContentClassesProperty);
        set => SetValue(StartContentClassesProperty, value);
    }

    public string ContentClasses
    {
        get => GetValue(ContentClassesProperty);
        set => SetValue(ContentClassesProperty, value);
    }

    public string EndContentClasses
    {
        get => GetValue(EndContentClassesProperty);
        set => SetValue(EndContentClassesProperty, value);
    }
}