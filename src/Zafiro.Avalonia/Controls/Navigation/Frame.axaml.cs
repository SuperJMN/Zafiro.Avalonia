using System.Reactive.Disposables;
using System.Windows.Input;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Zafiro.Avalonia.Controls.Navigation;

[TemplatePart("BackButton", typeof(EnhancedButton))]
[TemplatePart("Content", typeof(ContentPresenter))]
[TemplatePart("Header", typeof(ContentPresenter))]
[TemplatePart("Footer", typeof(ContentPresenter))]
public class Frame : ContentControl
{
    private readonly FrameBackCommand effectiveBackCommand;
    private readonly List<BackParticipantRegistration> backParticipants = [];
    private IDisposable contentParticipantRegistration = Disposable.Empty;
    private ICommand? observedBackCommand;
    private long nextActivationOrder;
    private long nextRegistrationOrder;

    public static readonly StyledProperty<BoxShadows> BoxShadowProperty = Border.BoxShadowProperty.AddOwner<Frame>();

    public static readonly StyledProperty<ICommand> BackCommandProperty = AvaloniaProperty.Register<Frame, ICommand>(
        nameof(BackCommand));

    public static readonly DirectProperty<Frame, ICommand> EffectiveBackCommandProperty = AvaloniaProperty.RegisterDirect<Frame, ICommand>(
        nameof(EffectiveBackCommand), frame => frame.EffectiveBackCommand);

    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<Frame, object?>(
        nameof(Header));

    public static readonly StyledProperty<object?> FooterProperty = AvaloniaProperty.Register<Frame, object?>(
        nameof(Footer));

    public static readonly StyledProperty<FrameHeaderDisplayMode> HeaderDisplayModeProperty = AvaloniaProperty.Register<Frame, FrameHeaderDisplayMode>(
        nameof(HeaderDisplayMode));

    public static readonly StyledProperty<IBrush?> HeaderBackgroundProperty = AvaloniaProperty.Register<Frame, IBrush?>(
        nameof(HeaderBackground));

    public static readonly StyledProperty<IBrush?> ContentBackgroundProperty = AvaloniaProperty.Register<Frame, IBrush?>(
        nameof(ContentBackground));

    public static readonly StyledProperty<IBrush?> FooterBackgroundProperty = AvaloniaProperty.Register<Frame, IBrush?>(
        nameof(FooterBackground));

    public static readonly StyledProperty<Thickness?> HeaderPaddingProperty = AvaloniaProperty.Register<Frame, Thickness?>(
        nameof(HeaderPadding));

    public static readonly StyledProperty<Thickness?> ContentPaddingProperty = AvaloniaProperty.Register<Frame, Thickness?>(
        nameof(ContentPadding));

    public static readonly StyledProperty<Thickness?> FooterPaddingProperty = AvaloniaProperty.Register<Frame, Thickness?>(
        nameof(FooterPadding));

    public static readonly DirectProperty<Frame, Thickness> EffectiveHeaderPaddingProperty = AvaloniaProperty.RegisterDirect<Frame, Thickness>(
        nameof(EffectiveHeaderPadding), o => o.EffectiveHeaderPadding);

    public static readonly DirectProperty<Frame, Thickness> EffectiveContentPaddingProperty = AvaloniaProperty.RegisterDirect<Frame, Thickness>(
        nameof(EffectiveContentPadding), o => o.EffectiveContentPadding);

    public static readonly DirectProperty<Frame, Thickness> EffectiveFooterPaddingProperty = AvaloniaProperty.RegisterDirect<Frame, Thickness>(
        nameof(EffectiveFooterPadding), o => o.EffectiveFooterPadding);

    public Frame()
    {
        effectiveBackCommand = new FrameBackCommand(this);
    }

    public ICommand BackCommand
    {
        get => GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public ICommand EffectiveBackCommand => effectiveBackCommand;

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public FrameHeaderDisplayMode HeaderDisplayMode
    {
        get => GetValue(HeaderDisplayModeProperty);
        set => SetValue(HeaderDisplayModeProperty, value);
    }

    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    public IBrush? HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public IBrush? ContentBackground
    {
        get => GetValue(ContentBackgroundProperty);
        set => SetValue(ContentBackgroundProperty, value);
    }

    public IBrush? FooterBackground
    {
        get => GetValue(FooterBackgroundProperty);
        set => SetValue(FooterBackgroundProperty, value);
    }

    public Thickness? HeaderPadding
    {
        get => GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    public Thickness? ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    public Thickness? FooterPadding
    {
        get => GetValue(FooterPaddingProperty);
        set => SetValue(FooterPaddingProperty, value);
    }

    public Thickness EffectiveHeaderPadding => HeaderPadding ?? Padding;
    public Thickness EffectiveContentPadding => ContentPadding ?? Padding;
    public Thickness EffectiveFooterPadding => FooterPadding ?? Padding;

    public IDisposable RegisterBackParticipant(IFrameBackParticipant participant)
    {
        if (backParticipants.Any(registration => ReferenceEquals(registration.Participant, participant)))
        {
            return Disposable.Empty;
        }

        var registration = new BackParticipantRegistration(participant, nextRegistrationOrder++);
        backParticipants.Add(registration);

        EventHandler canExecuteChanged = (_, _) => effectiveBackCommand.RaiseCanExecuteChanged();
        participant.BackCommand.CanExecuteChanged += canExecuteChanged;

        registration.Subscription = new CompositeDisposable
        {
            participant.CanHandleBack
                .DistinctUntilChanged()
                .Subscribe(canHandle =>
                {
                    registration.CanHandleBack = canHandle;

                    if (canHandle)
                    {
                        registration.ActivationOrder = nextActivationOrder++;
                    }

                    effectiveBackCommand.RaiseCanExecuteChanged();
                }),
            Disposable.Create(() => participant.BackCommand.CanExecuteChanged -= canExecuteChanged),
        };

        effectiveBackCommand.RaiseCanExecuteChanged();

        return Disposable.Create(() =>
        {
            registration.Dispose();
            backParticipants.Remove(registration);
            effectiveBackCommand.RaiseCanExecuteChanged();
        });
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PaddingProperty || change.Property == HeaderPaddingProperty)
        {
            RaisePropertyChanged(EffectiveHeaderPaddingProperty, default, EffectiveHeaderPadding);
        }

        if (change.Property == PaddingProperty || change.Property == ContentPaddingProperty)
        {
            RaisePropertyChanged(EffectiveContentPaddingProperty, default, EffectiveContentPadding);
        }

        if (change.Property == PaddingProperty || change.Property == FooterPaddingProperty)
        {
            RaisePropertyChanged(EffectiveFooterPaddingProperty, default, EffectiveFooterPadding);
        }

        if (change.Property == BackCommandProperty)
        {
            ObserveExternalBackCommand(BackCommand);
            effectiveBackCommand.RaiseCanExecuteChanged();
        }

        if (change.Property == ContentProperty)
        {
            RegisterContentParticipant(Content as IFrameBackParticipant);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ObserveExternalBackCommand(BackCommand);
        RegisterContentParticipant(Content as IFrameBackParticipant);
        var topLevel = TopLevel.GetTopLevel(this);
        topLevel?.AddHandler(TopLevel.BackRequestedEvent, OnSystemBackRequested);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        topLevel?.RemoveHandler(TopLevel.BackRequestedEvent, OnSystemBackRequested);
        ObserveExternalBackCommand(null);
        contentParticipantRegistration.Dispose();
        contentParticipantRegistration = Disposable.Empty;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnSystemBackRequested(object? sender, RoutedEventArgs e)
    {
        if (EffectiveBackCommand.CanExecute(null))
        {
            EffectiveBackCommand.Execute(null);
            e.Handled = true;
        }
    }

    private bool CanExecuteEffectiveBack(object? parameter)
    {
        if (ActiveParticipant(parameter) is not null)
        {
            return true;
        }

        return BackCommand?.CanExecute(parameter) == true;
    }

    private void ExecuteEffectiveBack(object? parameter)
    {
        if (ActiveParticipant(parameter) is { } participant)
        {
            participant.BackCommand.Execute(parameter);
            effectiveBackCommand.RaiseCanExecuteChanged();
            return;
        }

        if (BackCommand?.CanExecute(parameter) == true)
        {
            BackCommand.Execute(parameter);
            effectiveBackCommand.RaiseCanExecuteChanged();
        }
    }

    private IFrameBackParticipant? ActiveParticipant(object? parameter)
    {
        return backParticipants
            .Where(registration => registration.CanHandleBack && registration.Participant.BackCommand.CanExecute(parameter))
            .OrderByDescending(registration => registration.ActivationOrder)
            .ThenByDescending(registration => registration.RegistrationOrder)
            .Select(registration => registration.Participant)
            .FirstOrDefault();
    }

    private void ObserveExternalBackCommand(ICommand? command)
    {
        if (ReferenceEquals(observedBackCommand, command))
        {
            return;
        }

        if (observedBackCommand is not null)
        {
            observedBackCommand.CanExecuteChanged -= OnExternalBackCanExecuteChanged;
        }

        observedBackCommand = command;

        if (observedBackCommand is not null)
        {
            observedBackCommand.CanExecuteChanged += OnExternalBackCanExecuteChanged;
        }
    }

    private void OnExternalBackCanExecuteChanged(object? sender, EventArgs e)
    {
        effectiveBackCommand.RaiseCanExecuteChanged();
    }

    private void RegisterContentParticipant(IFrameBackParticipant? participant)
    {
        contentParticipantRegistration.Dispose();
        contentParticipantRegistration = participant is null ? Disposable.Empty : RegisterBackParticipant(participant);
    }

    private sealed class FrameBackCommand(Frame frame) : ICommand
    {
        public bool CanExecute(object? parameter) => frame.CanExecuteEffectiveBack(parameter);

        public void Execute(object? parameter)
        {
            frame.ExecuteEffectiveBack(parameter);
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class BackParticipantRegistration(IFrameBackParticipant participant, long registrationOrder) : IDisposable
    {
        public IFrameBackParticipant Participant { get; } = participant;

        public long RegistrationOrder { get; } = registrationOrder;

        public bool CanHandleBack { get; set; }

        public long ActivationOrder { get; set; } = registrationOrder;

        public IDisposable Subscription { get; set; } = Disposable.Empty;

        public void Dispose()
        {
            Subscription.Dispose();
        }
    }
}
