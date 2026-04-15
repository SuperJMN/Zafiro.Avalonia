using ReactiveUI;
using Zafiro.Avalonia.Controls.Diagrams;

namespace TestApp.Samples.Flow;

public class FlowNode : ReactiveObject, IHaveLocation
{
    private bool isActive;
    private double left;
    private string name;
    private int processingPower;
    private double top;

    public FlowNode(string name)
    {
        this.name = name;
    }

    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    public bool IsActive
    {
        get => isActive;
        set => this.RaiseAndSetIfChanged(ref isActive, value);
    }

    public int ProcessingPower
    {
        get => processingPower;
        set => this.RaiseAndSetIfChanged(ref processingPower, value);
    }

    public double Left
    {
        get => left;
        set => this.RaiseAndSetIfChanged(ref left, value);
    }

    public double Top
    {
        get => top;
        set => this.RaiseAndSetIfChanged(ref top, value);
    }
}