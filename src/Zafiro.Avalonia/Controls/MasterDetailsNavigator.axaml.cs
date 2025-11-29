using System.Reactive;
using Avalonia.Controls.Primitives;
using DynamicData;
using DynamicData.Aggregation;

namespace Zafiro.Avalonia.Controls;

public class MasterDetailsNavigator : TemplatedControl
{
    private IDisposable? subscription;

    public MasterDetailsNavigator()
    {
        SourceCache<MasterDetailsView, int> source = new(navigator => navigator.GetHashCode());

        subscription = MessageBus.Current.Listen<RegisterNavigation>()
            .Do(navigation => source.AddOrUpdate(navigation.MasterDetailsView))
            .Subscribe();

        var backCommands = source
            .Connect()
            .AutoRefresh(x => x.AreDetailsShown)
            .AutoRefresh(x => x.IsCollapsed)
            .Filter(masterDetailsView => masterDetailsView is { AreDetailsShown: true, IsCollapsed: true })
            .Transform(masterDetailsView => ReactiveCommand.Create(masterDetailsView.HideDetails));

        CanNavigateBack = backCommands.Count().Select(i => i > 0);
        Back = backCommands.ToCollection().Select(x => x.LastOrDefault());

        IsBackButtonDisplayed = CanNavigateBack.Select(canGoBack => canGoBack && RequiresBackButton());
    }

    public IObservable<bool> IsBackButtonDisplayed { get; }
    public IObservable<ReactiveCommand<Unit, Unit>?> Back { get; }
    public IObservable<bool> CanNavigateBack { get; }

    private static bool RequiresBackButton()
    {
        return !(OperatingSystem.IsAndroid() || OperatingSystem.IsIOS());
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        subscription?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }
}