# Zafiro.Avalonia — Canonical Examples

> Copy-pasteable reference examples extracted from the actual codebase. Each section cites the source file.

---

## 1. Minimal Shell App (complete)

The smallest possible Zafiro.Avalonia application with sidebar navigation.

**Source**: `samples/MinimalShell/`

### App.axaml
```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:misc="clr-namespace:Zafiro.Avalonia.Misc;assembly=Zafiro.Avalonia"
             x:Class="MinimalShell.App"
             RequestedThemeVariant="Default">
    <Application.Styles>
        <FluentTheme />
        <StyleInclude Source="avares://Zafiro.Avalonia/Styles.axaml" />
    </Application.Styles>
    <Application.DataTemplates>
        <misc:DataTemplateInclude Source="avares://Zafiro.Avalonia/DataTemplates.axaml" />
        <NamingConventionGeneratedViewLocator />
    </Application.DataTemplates>
</Application>
```

### App.axaml.cs
```csharp
public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        IconControlProviderRegistry.Register(new ProjektankerIconControlProvider(), asDefault: true);
        var services = new ServiceCollection();
        services.AddZafiroShell(logger: logger);
        services.AddAllSectionsFromAttributes(logger);
        var provider = services.BuildServiceProvider();
        var shell = provider.GetRequiredService<IShell>();
        this.Connect(() => new ShellView(), _ => shell, () => new Window { Title = "App", Width = 900, Height = 600 });
        base.OnFrameworkInitializationCompleted();
    }
}
```

### Section ViewModel (9 lines)
```csharp
using Zafiro.UI.Shell.Utils;

[Section(icon: "fa-home", sortIndex: 0)]
public class HomeViewModel
{
    public string Greeting => "Welcome to the Zafiro Shell!";
}
```

### Section with IHaveHeader
```csharp
[Section(icon: "fa-circle-info", sortIndex: 3)]
public class AboutViewModel : IHaveHeader
{
    public IObservable<object> Header => Observable.Return<object>("About This App");
    public string Version => "1.0.0";
}
```

### Section View
```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:sections="clr-namespace:MinimalShell.Sections"
             x:Class="MinimalShell.Sections.HomeView"
             x:DataType="sections:HomeViewModel">
    <TextBlock Text="{Binding Greeting}" HorizontalAlignment="Center" VerticalAlignment="Center" />
</UserControl>
```

---

## 2. Navigator + Frame Pattern

**Source**: `samples/TestApp/TestApp/Shell/`

### MainViewModel.cs (4 lines)
```csharp
public class MainViewModel(INavigator navigator)
{
    public INavigator Navigator { get; } = navigator;
}
```

### MainView.axaml (14 lines)
```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:s="clr-namespace:TestApp.Shell"
             xmlns:nav="clr-namespace:Zafiro.Avalonia.Controls.Navigation;assembly=Zafiro.Avalonia"
             x:Class="TestApp.Shell.MainView" x:DataType="s:MainViewModel">
    <Frame Content="{Binding Navigator.Content^}"
           BackCommand="{Binding Navigator.Back}">
        <Interaction.Behaviors>
            <nav:AutoHeaderFooterBehavior />
        </Interaction.Behaviors>
    </Frame>
</UserControl>
```

### Navigation ViewModel
```csharp
[Section(icon: "mdi-chevron-right", sortIndex: 14)]
[SectionGroup("navigation", "Navigation & Dialogs")]
public class NavigationSampleViewModel(INavigator navigator) : ReactiveObject, IHaveHeader, IHaveFooter
{
    public ICommand Navigate => ReactiveCommand.CreateFromTask(() => navigator.Go<TargetViewModel>());
    public IObservable<object> Footer => Observable.Return("This is a Footer");
    public IObservable<object> Header => Observable.Return("This is a Header");
}
```

---

## 3. DynamicData Reactive Filtering

**Source**: `samples/TestApp/TestApp/Samples/HomeViewModel.cs`

```csharp
public partial class HomeViewModel : ReactiveObject
{
    private readonly SourceCache<SampleCard, string> allSamples = new(c => c.Name);
    private readonly ReadOnlyObservableCollection<SampleCard> filteredSamples;

    public HomeViewModel(IEnumerable<SampleCard> samples, INavigator navigator, HomeViewState state)
    {
        State = state;
        allSamples.AddOrUpdate(samples);

        Categories = samples.Select(s => s.Category).Distinct().OrderBy(c => c).ToList();

        var searchFilter = this.WhenAnyValue(x => x.State.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(300), RxApp.MainThreadScheduler)
            .Select(BuildSearchFilter);

        var categoryFilter = this.WhenAnyValue(x => x.State.SelectedCategory)
            .Select(BuildCategoryFilter);

        var combinedFilter = searchFilter
            .CombineLatest(categoryFilter, (sf, cf) => new Func<SampleCard, bool>(card => sf(card) && cf(card)));

        allSamples.Connect()
            .Filter(combinedFilter)
            .SortBy(c => c.Name)
            .Bind(out filteredSamples)
            .Subscribe();

        NavigateToSample = ReactiveCommand.CreateFromTask<SampleCard>(async card =>
        {
            await navigator.Go(card.ViewModelType);
        });
    }

    public ReadOnlyObservableCollection<SampleCard> FilteredSamples => filteredSamples;
    public ReactiveCommand<SampleCard, Unit> NavigateToSample { get; }
}
```

---

## 4. SlimWizard (Linear) with Validation

**Source**: `samples/TestApp/TestApp/Samples/SlimWizard/WizardViewModel.cs`

### Wizard Construction
```csharp
private static SlimWizard<(int result, string)> CreateWizard()
{
    return WizardBuilder
        .StartWith(() => new Page1ViewModel(), "Page 1")
            .NextWith(model => model.ReturnSomeInt.Enhance("Next"))
        .Then(number => new Page2ViewModel(number))
            .Next((vm, number) => (result: number, vm.Text!)).WhenValid()
        .Then(_ => new Page3ViewModel(), "Completed!")
            .Next((_, val) => val, "Close").WhenValid()
        .Build(StepKind.Completion);
}
```

### Dialog and Navigation Hosting
```csharp
// Show wizard in a dialog overlay
ShowWizardInDialog = ReactiveCommand.CreateFromTask(
    () => CreateWizard().ShowInDialog(dialog, "Wizard Title"));

// Show wizard as navigation pages
NavigateToWizard = ReactiveCommand.CreateFromTask(async () =>
{
    var wizard = CreateWizard();
    var cancel = ReactiveCommand.CreateFromTask(() => navigator.GoBack());
    var host = new NavigationWizardHost(wizard, cancel.Enhance("Cancel", "Cancel"));
    await navigator.Go(() => host);
    var result = await wizard.Finished.Select(Maybe.From).FirstOrDefaultAsync();
    await navigator.GoBack();
    return result;
});
```

### Wizard Page with Validation
**Source**: `samples/TestApp/TestApp/Samples/SlimWizard/Pages/Page1ViewModel.cs`

```csharp
public partial class Page1ViewModel : ReactiveValidationObject, IHaveTitle, IHaveHeader, IHaveFooter
{
    [Reactive] private int? number;

    public Page1ViewModel()
    {
        this.ValidationRule(x => x.Number, i => i % 2 == 0, "Number must be even");
        ReturnSomeInt = ReactiveCommand.CreateFromTask(async () =>
        {
            await Task.Delay(1000);
            return Result.Success(Number!.Value);
        }, this.IsValid()).Enhance();
    }

    public IEnhancedCommand<Result<int>> ReturnSomeInt { get; set; }
    public IObservable<object> Footer => Observable.Return("This is the footer");
    public IObservable<object> Header => Observable.Return("This is the header");

    // Reactive title: updates as the user types
    public IObservable<string> Title => this
        .WhenAnyValue(x => x.Number)
        .Select(n => n.HasValue ? $"Page (value: {n.Value})" : "Page (enter a number)");
}
```

---

## 5. GraphWizard (Branching)

**Source**: `samples/TestApp/TestApp/Samples/GraphWizard/GraphWizardSampleViewModel.cs`

```csharp
private static GraphWizard<string> CreateWizard()
{
    var graph = GraphWizard.For<string>();

    // End node (typed finish)
    var endNode = graph.Step(new GenericStepViewModel("Finished!"), "End")
        .Finish(vm => "Done", nextLabel: "Finish!")
        .Build();

    // Branch A
    var nodeA = graph.Step(new GenericStepViewModel("Path A"), "Path A")
        .Next(vm => endNode, nextLabel: "Complete A")
        .Build();

    // Branch B
    var nodeB = graph.Step(new GenericStepViewModel("Path B"), "Path B")
        .Next(vm => endNode, nextLabel: "Complete B")
        .Build();

    // Start node with dynamic branching
    var start = new Step1ViewModel();
    var dynamicLabel = start.WhenAnyValue(x => x.Choice)
        .Select(choice => choice switch
        {
            "A" => "Choose A",
            "B" => "Choose B",
            _ => "Choose..."
        });

    var startNode = graph.Step(start, "Start")
        .Next(vm => vm.Choice == "A" ? nodeA : nodeB,
            canExecute: start.WhenAnyValue(x => x.Choice).NotNull(),
            nextLabel: dynamicLabel)
        .Build();

    return new GraphWizard<string>(startNode);
}
```

---

## 6. Dialog Patterns (All Overloads)

**Source**: `samples/TestApp/TestApp/Samples/Dialogs/DialogSampleViewModel.cs`

```csharp
// Simple message dialog
await dialogService.ShowMessage("Title", "Message body text");

// Message with tone (Warning / Error / Success)
await dialogService.ShowMessage("Warning", "Be careful!", icon: "⚠️", tone: DialogTone.Warning);
await dialogService.ShowMessage("Error", "Something broke!", icon: "❌", tone: DialogTone.Error);

// Form dialog returning Maybe<T>
ShowDialog = ReactiveCommand.CreateFromTask(async () =>
{
    return await dialogService.ShowAndGetResult(
        new MyViewModel(dialogService),
        "Title",
        model => model.IsValid(),
        model => model.Text);
});

// Handle present/absent results
ShowDialog.Values()
    .SelectMany(x => notificationService.Show($"Result: {x}", "Done").ToSignal())
    .Subscribe();
ShowDialog.Empties()
    .SelectMany(_ => notificationService.Show("Dismissed", "Info").ToSignal())
    .Subscribe();

// Custom options dialog
await dialogService.Show("Info", closeable => [
    new Option("Got it!", ReactiveCommand.Create(closeable.Close).Enhance(),
        new Settings { Icon = "✔️", IsDefault = true })
], icon: "💡", tone: DialogTone.Success);

// Submit-based dialog (custom command returns value)
await dialogService.ShowAndGetResult(new SubmitterViewModel(), "Title", model => model.Submit);
```

---

## 7. MasterDetailsView

**Source**: `samples/TestApp/TestApp/Samples/MasterDetails/`

### ViewModel
```csharp
public partial class MasterDetailsSampleViewModel : ReactiveObject
{
    [Reactive] private SampleSection? selectedSection;

    public MasterDetailsSampleViewModel()
    {
        Sections = new List<SampleSection>
        {
            new() { Title = "Sample 1", Content = "Content 1" },
            new() { Title = "Sample 2", Content = "Content 2" },
        };
    }

    public IEnumerable<SampleSection> Sections { get; }
}
```

---

## 8. AXAML Binding Patterns

**Source**: `samples/TestApp/TestApp/Samples/HomeView.axaml`

### Parent binding with type cast (inside DataTemplate)
```xml
<ItemsControl ItemsSource="{Binding FilteredSamples}">
    <ItemsControl.ItemTemplate>
        <DataTemplate x:DataType="samples:SampleCard">
            <Button Command="{Binding $parent[UserControl].((samples:HomeViewModel)DataContext).NavigateToSample}"
                    CommandParameter="{Binding}" />
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

### Async observable binding
```xml
<!-- The ^ operator subscribes to IObservable<T> -->
<Frame Content="{Binding Navigator.Content^}" />
```

### Utility class composition
```xml
<TextBlock Text="Title" Classes="Size-XL Weight-Bold" />
<TextBlock Text="Muted" Classes="Size-S Text-Muted" />
<Border Classes="Card Elevate" Padding="16" CornerRadius="8" />
<EnhancedButton Content="Action" Classes="Ghost" />
```

---

## 9. Full DI Composition Root

**Source**: `samples/TestApp/TestApp/CompositionRoot.cs`

```csharp
public static MainViewModel Create(Control view)
{
    ServiceCollection services = new();

    var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

    // Source-generated section registration
    services.AddAllSectionsFromAttributes(logger);

    // Services
    services.AddSingleton(DialogService.Create());
    services.AddSingleton<ILauncherService, LauncherService>();
    services.AddSingleton<INotificationService>(new NotificationService());

    // ViewModels
    services.AddTransient<MainViewModel>();

    // Navigator
    services.AddSingleton<INavigator>(sp =>
        new Navigator(sp, ((ILogger)logger).AsMaybe(), RxApp.MainThreadScheduler));

    // Platform services
    services.AddSingleton<IFileSystemPicker>(_ =>
        new AvaloniaFileSystemPicker(TopLevel.GetTopLevel(view).StorageProvider));

    var serviceProvider = services.BuildServiceProvider();

    // Set initial page
    var navigator = serviceProvider.GetRequiredService<INavigator>();
    navigator.SetInitialPage(() => serviceProvider.GetRequiredService<HomeViewModel>());

    return serviceProvider.GetRequiredService<MainViewModel>();
}
```

---

## 10. Cross-Platform Entry Points

### Desktop
```csharp
public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect().WithInterFont().LogToTrace().UseReactiveUI();
```

### Browser (WASM)
```csharp
internal partial class Program
{
    private static async Task Main(string[] args)
        => await BuildAvaloniaApp().StartBrowserAppAsync("out");
}
```

### Android
```csharp
[Activity(Theme = "@style/MyTheme.NoActionBar", MainLauncher = true)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder).UseReactiveUI();
}
```

### iOS
```csharp
public class AppDelegate : AvaloniaAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder).UseReactiveUI();
}
```

**Source**: `samples/TestApp/TestApp.Desktop/Program.cs`, `TestApp.Browser/Program.cs`, `TestApp.Android/MainActivity.cs`, `TestApp.iOS/AppDelegate.cs`.

---

## 11. Responsive Layout Patterns

Three patterns that cover 90% of adaptive UI needs. Combine them for full-page responsive layouts.

**Source**: `samples/TestApp/TestApp/Samples/Panels/PanelsView.axaml`, `samples/TestApp/TestApp/Samples/Layout/FlexPanelView.axaml`

### Responsive Card Grid (BootstrapGridPanel)

```xml
<!-- xmlns:panels="clr-namespace:Zafiro.Avalonia.Controls.Panels;assembly=Zafiro.Avalonia" -->

<!-- Mobile: 1 col. Tablet: 2 cols. Desktop: 3 cols. -->
<panels:BootstrapGridPanel MaxColumns="12" Gutter="16" FluidContainer="True">
    <Border panels:BootstrapGridPanel.Col="12"
            panels:BootstrapGridPanel.ColMd="6"
            panels:BootstrapGridPanel.ColLg="4"
            Background="#2196F3" CornerRadius="8" Padding="16">
        <TextBlock Text="Card 1" Foreground="White" />
    </Border>
    <Border panels:BootstrapGridPanel.Col="12"
            panels:BootstrapGridPanel.ColMd="6"
            panels:BootstrapGridPanel.ColLg="4"
            Background="#4CAF50" CornerRadius="8" Padding="16">
        <TextBlock Text="Card 2" Foreground="White" />
    </Border>
    <Border panels:BootstrapGridPanel.Col="12"
            panels:BootstrapGridPanel.ColMd="6"
            panels:BootstrapGridPanel.ColLg="4"
            Background="#FF9800" CornerRadius="8" Padding="16">
        <TextBlock Text="Card 3" Foreground="White" />
    </Border>
</panels:BootstrapGridPanel>
```

### Adaptive Toolbar (FlexPanel)

```xml
<!-- Toolbar: menu left, title stretches, actions right. Wraps on narrow. -->
<panels:FlexPanel Direction="Row" Wrap="Wrap" AlignItems="Center" Gap="8">
    <Button panels:FlexPanel.Shrink="0">☰</Button>
    <TextBlock panels:FlexPanel.Grow="1" Text="Dashboard"
               FontSize="18" FontWeight="Bold" />
    <Button panels:FlexPanel.Shrink="0"
            panels:FlexPanel.MarginLeftAuto="True">🔔</Button>
    <Button panels:FlexPanel.Shrink="0">👤</Button>
</panels:FlexPanel>
```

### Full-Page Responsive Layout (Combined)

```xml
<!-- Complete responsive page: header + sidebar/content + footer -->
<panels:BootstrapGridPanel MaxColumns="12" Gutter="16" FluidContainer="True">

    <!-- Header: always full width -->
    <panels:FlexPanel panels:BootstrapGridPanel.Col="12"
                      Direction="Row" AlignItems="Center" Gap="8">
        <Button panels:FlexPanel.Shrink="0">☰</Button>
        <TextBlock panels:FlexPanel.Grow="1" Text="My App" FontSize="18" />
        <Button panels:FlexPanel.Shrink="0">⚙</Button>
    </panels:FlexPanel>

    <!-- Sidebar: full width mobile, 3 cols desktop -->
    <Border panels:BootstrapGridPanel.Col="12"
            panels:BootstrapGridPanel.ColMd="3"
            panels:BootstrapGridPanel.OrderMd="1"
            Background="{DynamicResource SystemAltHighColor}" Padding="12">
        <StackPanel Spacing="8">
            <TextBlock Text="Navigation" FontWeight="Bold" />
            <Button HorizontalAlignment="Stretch">Home</Button>
            <Button HorizontalAlignment="Stretch">Settings</Button>
            <Button HorizontalAlignment="Stretch">About</Button>
        </StackPanel>
    </Border>

    <!-- Main content: full width mobile, 9 cols desktop -->
    <Border panels:BootstrapGridPanel.Col="12"
            panels:BootstrapGridPanel.ColMd="9"
            panels:BootstrapGridPanel.OrderMd="2"
            Padding="12">
        <!-- Nested responsive grid for content cards -->
        <panels:BootstrapGridPanel MaxColumns="12" Gutter="12" FluidContainer="True">
            <Border panels:BootstrapGridPanel.Col="12"
                    panels:BootstrapGridPanel.ColLg="6"
                    Background="#E3F2FD" CornerRadius="8" Padding="16">
                <TextBlock Text="Widget A" />
            </Border>
            <Border panels:BootstrapGridPanel.Col="12"
                    panels:BootstrapGridPanel.ColLg="6"
                    Background="#E8F5E9" CornerRadius="8" Padding="16">
                <TextBlock Text="Widget B" />
            </Border>
        </panels:BootstrapGridPanel>
    </Border>

    <!-- Footer: always full width -->
    <Border panels:BootstrapGridPanel.Col="12"
            panels:BootstrapGridPanel.OrderMd="3"
            Padding="12">
        <TextBlock Text="© 2026 My App" HorizontalAlignment="Center"
                   Foreground="{DynamicResource SystemBaseMediumColor}" />
    </Border>

</panels:BootstrapGridPanel>
```
