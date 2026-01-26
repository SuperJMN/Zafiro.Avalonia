using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Serilog;
using Zafiro.Avalonia.Controls.Wizards.Slim;
using Zafiro.UI.Commands;
using Zafiro.UI.Navigation;
using Zafiro.UI.Wizards.Slim;
using Zafiro.UI.Wizards.Slim.Builder;

namespace Zafiro.Avalonia.Tests;

public class SlimWizardScenarioTests
{
    [Fact]
    public void Wizard_requires_at_least_one_page()
    {
        Assert.Throws<ArgumentException>(() => new SlimWizard<object>(Array.Empty<IWizardStep>()));
    }

    [Fact]
    public async Task Last_page_next_finishes_and_emits_result()
    {
        var next = ReactiveCommand.Create(() => Result.Success((object)"done")).Enhance();
        var step = new WizardStep(
            StepKind.Completion,
            "Done",
            _ => new object(),
            _ => next,
            _ => Observable.Return("Done"));

        var wizard = new SlimWizard<string>(new List<IWizardStep> { step }, ImmediateScheduler.Instance);

        var finishedTask = wizard.Finished.Take(1).ToTask();
        ((ICommand)wizard.Next).Execute(null);

        var result = await finishedTask;
        Assert.Equal("done", result);
    }

    [Fact]
    public void First_page_cannot_go_back()
    {
        var step1Next = ReactiveCommand.Create(() => Result.Success((object)"x")).Enhance();
        var step2Next = ReactiveCommand.Create(() => Result.Success((object)"y")).Enhance();

        var steps = new List<IWizardStep>
        {
            new WizardStep(StepKind.Normal, "1", _ => new object(), _ => step1Next, _ => Observable.Return("1")),
            new WizardStep(StepKind.Completion, "2", _ => new object(), _ => step2Next, _ => Observable.Return("2")),
        };

        var wizard = new SlimWizard<string>(steps, ImmediateScheduler.Instance);

        Assert.False(((ICommand)wizard.Back).CanExecute(null));
        Assert.Equal(0, wizard.CurrentStepIndex);
    }

    [Fact]
    public async Task Next_is_enabled_only_when_current_step_command_allows_it()
    {
        var canExecute = new BehaviorSubject<bool>(false);
        var next = ReactiveCommand.Create(() => Result.Success((object)"done"), canExecute).Enhance();

        var step = new WizardStep(
            StepKind.Completion,
            "Done",
            _ => new object(),
            _ => next,
            _ => Observable.Return("Done"));

        var wizard = new SlimWizard<string>(new List<IWizardStep> { step }, ImmediateScheduler.Instance);

        Assert.False(((ICommand)wizard.Next).CanExecute(null));

        canExecute.OnNext(true);
        await ((IReactiveCommand)wizard.Next).CanExecute.FirstAsync(x => x).ToTask();

        Assert.True(((ICommand)wizard.Next).CanExecute(null));
    }

    [Fact]
    public async Task Navigator_content_is_wizard_while_running_and_returns_to_initial_on_finish()
    {
        var initialContent = new object();
        var navigator = CreateNavigator(ImmediateScheduler.Instance);

        object? current = null;
        using var _ = navigator.Content.Subscribe(x => current = x);
        navigator.SetInitialPage(() => initialContent);

        var wizard = CreateSingleStepWizard("finished");
        using var session = new WizardNavigationSession<string>(wizard, navigator, _ => wizard);

        var start = await session.StartAsync();
        Assert.True(start.IsSuccess);
        Assert.Same(wizard, current);

        ((ICommand)wizard.Next).Execute(null);

        var result = await session.Completion;
        Assert.True(result.HasValue);
        Assert.Equal("finished", result.Value);
        Assert.Same(initialContent, current);
    }

    [Fact]
    public async Task Navigator_returns_to_initial_on_cancel()
    {
        var initialContent = new object();
        var navigator = CreateNavigator(ImmediateScheduler.Instance);

        object? current = null;
        using var _ = navigator.Content.Subscribe(x => current = x);
        navigator.SetInitialPage(() => initialContent);

        var wizard = CreateSingleStepWizard("finished");
        using var session = new WizardNavigationSession<string>(wizard, navigator, _ => wizard);

        var start = await session.StartAsync();
        Assert.True(start.IsSuccess);
        Assert.Same(wizard, current);

        session.Cancel.Execute(null);

        var result = await session.Completion;
        Assert.False(result.HasValue);
        Assert.Same(initialContent, current);
    }

    [Fact]
    public async Task Navigate_extension_returns_to_initial_on_finish()
    {
        var initialContent = new object();
        var navigator = CreateNavigator(ImmediateScheduler.Instance);

        object? current = null;
        using var _ = navigator.Content.Subscribe(x => current = x);
        navigator.SetInitialPage(() => initialContent);

        var wizard = CreateSingleStepWizard("finished");

        var completion = wizard.Navigate(navigator);

        await WaitUntilAsync(() => current is ISlimWizard, TimeSpan.FromSeconds(2));
        Assert.IsType<SlimWizard<string>>(current);

        ((ICommand)wizard.Next).Execute(null);

        var result = await completion;
        Assert.True(result.HasValue);
        Assert.Equal("finished", result.Value);
        Assert.Same(initialContent, current);
    }


    [Fact]
    public async Task Nested_wizard_using_Navigate_extension_returns_to_parent_then_initial_on_finish()
    {
        var initialContent = new object();
        var navigator = CreateNavigator(ImmediateScheduler.Instance);

        object? current = null;
        using var _ = navigator.Content.Subscribe(x => current = x);
        navigator.SetInitialPage(() => initialContent);

        SlimWizard<string>? childWizard = null;

        var parentSteps = new List<IWizardStep>
        {
            new WizardStep(
                StepKind.Normal,
                "Parent",
                _ => new object(),
                _ => ReactiveCommand.CreateFromTask(async () =>
                {
                    childWizard = CreateSingleStepWizard("child-result");

                    var maybe = await childWizard.Navigate(navigator);
                    return maybe.HasValue
                        ? Result.Success((object)maybe.Value)
                        : Result.Failure<object>("Child canceled");
                }).Enhance(),
                _ => Observable.Return("Parent")),
            new WizardStep(
                StepKind.Completion,
                "Finish",
                _ => new object(),
                _ => ReactiveCommand.Create(() => Result.Success((object)"parent-finished")).Enhance(),
                _ => Observable.Return("Finish")),
        };

        var parentWizard = new SlimWizard<string>(parentSteps, ImmediateScheduler.Instance);

        var completion = parentWizard.Navigate(navigator);

        await WaitUntilAsync(
            () => current is ISlimWizard w && ReferenceEquals(w, parentWizard),
            TimeSpan.FromSeconds(2));

        var parentNextExecution = parentWizard.TypedNext.Execute().ToTask();

        await WaitUntilAsync(
            () => childWizard is not null
                  && current is ISlimWizard w
                  && ReferenceEquals(w, childWizard),
            TimeSpan.FromSeconds(2));

        ((ICommand)childWizard!.Next).Execute(null);

        await WaitUntilAsync(
            () => current is ISlimWizard w && ReferenceEquals(w, parentWizard),
            TimeSpan.FromSeconds(2));

        var parentNextResult = await parentNextExecution;
        Assert.True(parentNextResult.IsSuccess);
        Assert.Equal(1, parentWizard.CurrentStepIndex);

        ((ICommand)parentWizard.Next).Execute(null);
        var parentResult = await completion;

        Assert.True(parentResult.HasValue);
        Assert.Equal("parent-finished", parentResult.Value);
        Assert.Same(initialContent, current);
    }

    [Fact]
    public async Task Nested_wizard_navigates_to_child_and_back_to_parent_then_initial_on_finish()
    {
        var initialContent = new object();
        var navigator = CreateNavigator(ImmediateScheduler.Instance);

        object? current = null;
        using var _ = navigator.Content.Subscribe(x => current = x);
        navigator.SetInitialPage(() => initialContent);

        SlimWizard<string>? childWizard = null;

        var parentSteps = new List<IWizardStep>
        {
            new WizardStep(
                StepKind.Normal,
                "Parent",
                _ => new object(),
                _ => ReactiveCommand.CreateFromTask(async () =>
                {
                    childWizard = CreateSingleStepWizard("child-result");
                    using var childSession = new WizardNavigationSession<string>(childWizard, navigator, __ => childWizard);
                    var start = await childSession.StartAsync();
                    if (start.IsFailure)
                    {
                        return Result.Failure<object>(start.Error);
                    }

                    var maybe = await childSession.Completion;
                    return maybe.HasValue
                        ? Result.Success((object)maybe.Value)
                        : Result.Failure<object>("Child canceled");
                }).Enhance(),
                _ => Observable.Return("Parent")),
            new WizardStep(
                StepKind.Completion,
                "Finish",
                _ => new object(),
                _ => ReactiveCommand.Create(() => Result.Success((object)"parent-finished")).Enhance(),
                _ => Observable.Return("Finish")),
        };

        var parentWizard = new SlimWizard<string>(parentSteps, ImmediateScheduler.Instance);
        using var parentSession = new WizardNavigationSession<string>(parentWizard, navigator, _ => parentWizard);

        var startParent = await parentSession.StartAsync();
        Assert.True(startParent.IsSuccess);
        Assert.Same(parentWizard, current);

        var parentNextExecution = parentWizard.TypedNext.Execute().ToTask();

        await WaitUntilAsync(() => childWizard != null && ReferenceEquals(current, childWizard), TimeSpan.FromSeconds(2));
        Assert.NotNull(childWizard);
        Assert.Same(childWizard, current);

        ((ICommand)childWizard!.Next).Execute(null);

        await WaitUntilAsync(() => ReferenceEquals(current, parentWizard), TimeSpan.FromSeconds(2));
        var parentNextResult = await parentNextExecution;
        Assert.True(parentNextResult.IsSuccess);
        Assert.Equal(1, parentWizard.CurrentStepIndex);

        ((ICommand)parentWizard.Next).Execute(null);
        var parentResult = await parentSession.Completion;

        Assert.True(parentResult.HasValue);
        Assert.Equal("parent-finished", parentResult.Value);
        Assert.Same(initialContent, current);
    }

    [Fact]
    public async Task Cancel_child_then_cancel_parent_returns_to_initial()
    {
        var initialContent = new object();
        var navigator = CreateNavigator(ImmediateScheduler.Instance);

        object? current = null;
        using var _ = navigator.Content.Subscribe(x => current = x);
        navigator.SetInitialPage(() => initialContent);

        SlimWizard<string>? childWizard = null;
        WizardNavigationSession<string>? childSession = null;

        var parentSteps = new List<IWizardStep>
        {
            new WizardStep(
                StepKind.Normal,
                "Parent",
                _ => new object(),
                _ => ReactiveCommand.CreateFromTask(async () =>
                {
                    childWizard = CreateSingleStepWizard("child-result");
                    childSession = new WizardNavigationSession<string>(childWizard, navigator, __ => childWizard);

                    var start = await childSession.StartAsync();
                    if (start.IsFailure)
                    {
                        return Result.Failure<object>(start.Error);
                    }

                    var maybe = await childSession.Completion;
                    childSession.Dispose();
                    childSession = null;

                    return maybe.HasValue
                        ? Result.Success((object)maybe.Value)
                        : Result.Failure<object>("Child canceled");
                }).Enhance(),
                _ => Observable.Return("Parent")),
            new WizardStep(
                StepKind.Completion,
                "Finish",
                _ => new object(),
                _ => ReactiveCommand.Create(() => Result.Success((object)"parent-finished")).Enhance(),
                _ => Observable.Return("Finish")),
        };

        var parentWizard = new SlimWizard<string>(parentSteps, ImmediateScheduler.Instance);
        using var parentSession = new WizardNavigationSession<string>(parentWizard, navigator, _ => parentWizard);

        var startParent = await parentSession.StartAsync();
        Assert.True(startParent.IsSuccess);
        Assert.Same(parentWizard, current);

        var parentNextExecution = parentWizard.TypedNext.Execute().ToTask();

        await WaitUntilAsync(() => childWizard != null && ReferenceEquals(current, childWizard), TimeSpan.FromSeconds(2));
        Assert.NotNull(childWizard);
        Assert.Same(childWizard, current);

        Assert.NotNull(childSession);
        childSession!.Cancel.Execute(null);

        await parentNextExecution;

        Assert.Equal(0, parentWizard.CurrentStepIndex);
        Assert.Same(parentWizard, current);

        parentSession.Cancel.Execute(null);
        var parentResult = await parentSession.Completion;

        Assert.False(parentResult.HasValue);
        Assert.Same(initialContent, current);
    }

    private static SlimWizard<string> CreateSingleStepWizard(string result)
    {
        var next = ReactiveCommand.Create(() => Result.Success((object)result)).Enhance();
        var step = new WizardStep(
            StepKind.Completion,
            "Done",
            _ => new object(),
            _ => next,
            _ => Observable.Return("Done"));

        return new SlimWizard<string>(new List<IWizardStep> { step }, ImmediateScheduler.Instance);
    }

    private static Navigator CreateNavigator(IScheduler scheduler)
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        return new Navigator(serviceProvider, Maybe<ILogger>.None, scheduler);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();
        while (!condition())
        {
            if (sw.Elapsed > timeout)
            {
                throw new TimeoutException("Condition not met within timeout.");
            }

            await Task.Delay(1);
        }
    }
}