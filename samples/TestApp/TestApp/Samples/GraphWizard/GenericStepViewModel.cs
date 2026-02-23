using ReactiveUI;

namespace TestApp.Samples.GraphWizard;

public class GenericStepViewModel : ReactiveObject
{
    public GenericStepViewModel(string message)
    {
        Message = message;
    }

    public string Message { get; }
}