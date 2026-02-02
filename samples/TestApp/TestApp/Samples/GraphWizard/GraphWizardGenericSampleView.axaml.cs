using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TestApp.Samples.GraphWizard;

public partial class GraphWizardGenericSampleView : UserControl
{
    public GraphWizardGenericSampleView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}