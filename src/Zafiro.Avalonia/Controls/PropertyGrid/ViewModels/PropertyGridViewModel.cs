using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reflection;
using DynamicData;

namespace Zafiro.Avalonia.Controls.PropertyGrid.ViewModels;

public class PropertyGridViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable disposables = new();
    private readonly ReadOnlyObservableCollection<IPropertyItem> properties;

    public PropertyGridViewModel(IObservable<IList<object>> targets)
    {
        var propertiesSource = new SourceList<IPropertyItem>().DisposeWith(disposables);

        targets
            .Select(list =>
            {
                if (list is INotifyCollectionChanged incc)
                {
                    return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                            h => incc.CollectionChanged += h,
                            h => incc.CollectionChanged -= h)
                        .Select(_ => list)
                        .StartWith(list);
                }

                return Observable.Return(list);
            })
            .Switch()
            .Select(GetCommonProperties)
            .Subscribe(list =>
            {
                propertiesSource.Edit(inner =>
                {
                    inner.Clear();
                    inner.AddRange(list);
                });
            })
            .DisposeWith(disposables);

        propertiesSource
            .Connect()
            .Bind(out properties)
            .Subscribe()
            .DisposeWith(disposables);
    }

    public ReadOnlyObservableCollection<IPropertyItem> Properties => properties;

    public void Dispose()
    {
        disposables.Dispose();
    }

    private IEnumerable<IPropertyItem> GetCommonProperties(IList<object> objects)
    {
        if (!objects.Any())
        {
            return Enumerable.Empty<IPropertyItem>();
        }

        var first = objects.First();
        var candidates = first.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.CanRead);

        var common = candidates.Where(prop =>
            objects.All(obj =>
            {
                var p = obj.GetType().GetProperty(prop.Name);
                return p != null && p.PropertyType == prop.PropertyType && p.CanWrite && p.CanRead;
            }));

        return common.Select(info => new PropertyItem(info.Name, info.PropertyType, objects));
    }
}