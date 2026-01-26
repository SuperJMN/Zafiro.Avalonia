using Zafiro.UI.Navigation;

namespace TestApp.Samples.Navigation;

public class TargetViewModel : IHaveHeader, IHaveFooter
{
    public object Footer => "Footer (Target)";
    public object Header => "Header (Target)";
}