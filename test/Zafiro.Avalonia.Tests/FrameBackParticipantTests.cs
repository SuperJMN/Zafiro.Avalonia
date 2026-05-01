using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Zafiro.Avalonia.Controls;
using Zafiro.Avalonia.Controls.Navigation;

namespace Zafiro.Avalonia.Tests;

public class FrameBackParticipantTests
{
    [AvaloniaFact]
    public void Frame_uses_nested_back_before_external_back()
    {
        var externalBack = new CountingCommand();
        var participant = new TestBackParticipant(canHandleBack: true);
        var frame = Attach(new Frame
        {
            BackCommand = externalBack,
            Content = participant,
        });

        frame.EffectiveBackCommand.Execute(null);

        Assert.Equal(1, participant.ExecuteCount);
        Assert.Equal(0, externalBack.ExecuteCount);
    }

    [AvaloniaFact]
    public void Frame_falls_back_to_external_back_after_nested_back_closes_details()
    {
        var externalBack = new CountingCommand();
        var participant = new TestBackParticipant(canHandleBack: true, canHandleBackAfterExecute: false);
        var frame = Attach(new Frame
        {
            BackCommand = externalBack,
            Content = participant,
        });

        frame.EffectiveBackCommand.Execute(null);
        frame.EffectiveBackCommand.Execute(null);

        Assert.Equal(1, participant.ExecuteCount);
        Assert.Equal(1, externalBack.ExecuteCount);
    }

    [AvaloniaFact]
    public void Frame_uses_nested_visual_tree_participant()
    {
        var externalBack = new CountingCommand();
        var masterDetails = new MasterDetailsView
        {
            CompactWidth = 500,
            ItemsSource = new[] { "One" },
            SelectedItem = "One",
            AreDetailsShown = true,
        };
        var frame = Attach(new Frame
        {
            BackCommand = externalBack,
            Content = new Border
            {
                Child = masterDetails,
            },
        });

        frame.EffectiveBackCommand.Execute(null);

        Assert.False(masterDetails.AreDetailsShown);
        Assert.Equal(0, externalBack.ExecuteCount);
    }

    [AvaloniaFact]
    public void Direct_visual_tree_participant_is_not_registered_twice()
    {
        var participant = new SelfRegisteringBackParticipant();

        Attach(new Frame
        {
            Content = participant,
        });

        Assert.Equal(1, participant.SubscriptionCount);
    }

    [AvaloniaFact]
    public void Detached_participant_is_unregistered()
    {
        var externalBack = new CountingCommand();
        var participant = new TestBackParticipant(canHandleBack: true);
        var frame = Attach(new Frame
        {
            BackCommand = externalBack,
            Content = participant,
        });

        frame.Content = null;

        frame.EffectiveBackCommand.Execute(null);

        Assert.Equal(0, participant.ExecuteCount);
        Assert.Equal(1, externalBack.ExecuteCount);
    }

    [AvaloniaFact]
    public void Participant_outside_frame_is_not_used()
    {
        var externalBack = new CountingCommand();
        var inside = new TextBlock();
        var outside = new TestBackParticipant(canHandleBack: true);
        var frame = Attach(new Frame
        {
            BackCommand = externalBack,
            Content = inside,
        });

        Attach(outside);

        frame.EffectiveBackCommand.Execute(null);

        Assert.Equal(0, outside.ExecuteCount);
        Assert.Equal(1, externalBack.ExecuteCount);
    }

    private static T Attach<T>(T control)
        where T : Control
    {
        var window = new Window
        {
            Content = control,
            Width = 400,
            Height = 400,
        };

        window.Show();
        control.Measure(new Size(400, 400));
        control.Arrange(new Rect(0, 0, 400, 400));

        return control;
    }

    private sealed class TestBackParticipant : ContentControl, IFrameBackParticipant
    {
        private readonly bool canHandleBackAfterExecute;

        public TestBackParticipant(bool canHandleBack, bool canHandleBackAfterExecute = true)
        {
            this.canHandleBackAfterExecute = canHandleBackAfterExecute;
            CanHandleBack = this.GetObservable(CanHandleBackProperty);
            BackCommand = new DelegateCommand(HandleBack);
            SetValue(CanHandleBackProperty, canHandleBack);
        }

        public static readonly StyledProperty<bool> CanHandleBackProperty =
            AvaloniaProperty.Register<TestBackParticipant, bool>(nameof(CanHandleBack));

        public IObservable<bool> CanHandleBack { get; }

        public ICommand BackCommand { get; }

        public int ExecuteCount { get; private set; }

        private void HandleBack()
        {
            ExecuteCount++;
            SetValue(CanHandleBackProperty, canHandleBackAfterExecute);
        }
    }

    private sealed class SelfRegisteringBackParticipant : ContentControl, IFrameBackParticipant
    {
        private readonly BehaviorSubject<bool> canHandleBack = new(true);
        private readonly DelegateCommand backCommand;
        private IDisposable registration = Disposable.Empty;

        public SelfRegisteringBackParticipant()
        {
            backCommand = new DelegateCommand(() => { });
        }

        public IObservable<bool> CanHandleBack => Observable.Create<bool>(observer =>
        {
            SubscriptionCount++;
            var subscription = canHandleBack.Subscribe(observer);

            return Disposable.Create(() =>
            {
                subscription.Dispose();
                SubscriptionCount--;
            });
        });

        public ICommand BackCommand => backCommand;

        public int SubscriptionCount { get; private set; }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            registration = this.FindAncestorOfType<Frame>()?.RegisterBackParticipant(this) ?? Disposable.Empty;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            registration.Dispose();
            registration = Disposable.Empty;
            base.OnDetachedFromVisualTree(e);
        }
    }

    private sealed class CountingCommand : ICommand
    {
        public int ExecuteCount { get; private set; }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }

    private sealed class DelegateCommand(Action execute) : ICommand
    {
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            execute();
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
