using ReactiveUI.SourceGenerators;
using Zafiro.UI.Navigation.Sections;

namespace Zafiro.Avalonia.Controls.Shell;

    public partial class ContentSectionDesign : ReactiveObject, ISection
    {
        [Reactive] private object? icon;
        [Reactive] private int sortOrder;

    public ContentSectionDesign()
    {
        // Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(2), AvaloniaScheduler.Instance)
        //     .Select(_ => Random.Shared.Next(30))
        //     .BindTo(this, x => x.SortOrder);
    }

        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public bool IsVisible { get; set; } = true;
        public IObservable<object> Content { get; set; }
        public SectionGroup? Group { get; set; }
    }