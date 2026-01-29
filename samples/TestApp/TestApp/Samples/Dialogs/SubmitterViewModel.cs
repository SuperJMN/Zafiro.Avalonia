using CSharpFunctionalExtensions;
using ReactiveUI;
using Zafiro.UI.Commands;

namespace TestApp.Samples.Dialogs;

public class SubmitterViewModel
{
    public SubmitterViewModel()
    {
        IEnhancedCommand<Result> p = EnhancedCommand.CreateWithResult(() => Result.Success(1)).AsResult();
        Submit = ReactiveCommand.Create(() => Result.Success(1)).Enhance();
    }

    public IEnhancedCommand<Result<int>> Submit { get; }
}