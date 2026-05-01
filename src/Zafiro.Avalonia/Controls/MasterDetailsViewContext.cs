using System.Collections;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Windows.Input;
using Avalonia.Controls.Templates;
using JetBrains.Annotations;

namespace Zafiro.Avalonia.Controls;

[PublicAPI]
public sealed class MasterDetailsViewContext : INotifyPropertyChanged, IDisposable
{
    private readonly MasterDetailsView owner;
    private readonly CompositeDisposable disposables = new();

    public MasterDetailsViewContext(MasterDetailsView owner)
    {
        this.owner = owner;

        Observe(MasterDetailsView.ItemsSourceProperty, nameof(ItemsSource));
        Observe(MasterDetailsView.SelectedItemProperty, nameof(SelectedItem));
        Observe(MasterDetailsView.AreDetailsShownProperty, nameof(AreDetailsShown));
        Observe(MasterDetailsView.IsCompactProperty, nameof(IsCompact));
        Observe(MasterDetailsView.ItemTemplateProperty, nameof(ItemTemplate));
        Observe(MasterDetailsView.CompactItemTemplateProperty, nameof(CompactItemTemplate));
        Observe(MasterDetailsView.EmptyTemplateProperty, nameof(EmptyTemplate));
        Observe(MasterDetailsView.MasterPaneWidthProperty, nameof(MasterPaneWidth));
        Observe(MasterDetailsView.HasItemsProperty, nameof(HasItems));
        Observe(MasterDetailsView.HasSelectedItemProperty, nameof(HasSelectedItem));
    }

    public IEnumerable? ItemsSource => owner.ItemsSource;

    public object? SelectedItem
    {
        get => owner.SelectedItem;
        set => owner.SelectedItem = value;
    }

    public ICommand OpenDetailsCommand => owner.OpenDetailsCommand;

    public ICommand CloseDetailsCommand => owner.CloseDetailsCommand;

    public bool AreDetailsShown => owner.AreDetailsShown;

    public bool IsCompact => owner.IsCompact;

    public bool HasItems => owner.HasItems;

    public bool HasSelectedItem => owner.HasSelectedItem;

    public double MasterPaneWidth => owner.MasterPaneWidth;

    public IDataTemplate? ItemTemplate => owner.ItemTemplate;

    public IDataTemplate? CompactItemTemplate => owner.CompactItemTemplate;

    public IDataTemplate? EmptyTemplate => owner.EmptyTemplate;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        disposables.Dispose();
    }

    private void Observe<T>(AvaloniaProperty<T> property, string propertyName)
    {
        owner.GetObservable(property)
            .Subscribe(_ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)))
            .DisposeWith(disposables);
    }
}
