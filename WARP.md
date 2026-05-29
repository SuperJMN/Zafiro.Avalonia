# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository (Zafiro.Avalonia).

Scope: repository root of Zafiro.Avalonia (UI library, controls, samples, platform heads).

Tech stack

- .NET 8 for libraries and samples; Android head targets net9.0-android
- Avalonia 11.3.x, ReactiveUI
- Zafiro core is developed in the sibling repo `../Zafiro`; CI and package builds consume published NuGet packages.

Common commands

- Restore
  - dotnet restore Zafiro.Avalonia.sln

- Build
  - Debug: dotnet build Zafiro.Avalonia.sln -c Debug
  - Release: dotnet build Zafiro.Avalonia.sln -c Release

- Run desktop sample(s)
  - dotnet run --project samples/TestApp/TestApp.Desktop/TestApp.Desktop.csproj -c Debug

- Android (net9.0-android)
  - Debug build: dotnet build samples/TestApp/TestApp.Android/TestApp.Android.csproj -c Debug
  - Release AOT (publishing): dotnet publish samples/TestApp/TestApp.Android/TestApp.Android.csproj -c Release
    - The Android head enables AOT + LLVM in Release via csproj properties.

- Browser (WASM)
  - dotnet run --project samples/TestApp/TestApp.Browser/TestApp.Browser.csproj -c Debug

Android AOT + XAML Behaviors

- Always prefer the Avalonia XML namespace URI for behaviors instead of assembly-qualified namespaces in XAML
  - Use xmlns:b="https://github.com/avaloniaui" and tags like b:DropTargetBehavior, b:DragDrop, etc.
  - Do not use xmlns with assembly=... for Avalonia.Xaml.Interactions.* or Xaml.Behaviors.Interactions.* — assembly-qualified loads can fail in trimmed/AOT builds.
- Consolidate behavior packages for Android
  - Prefer the Xaml.Behaviors.* family and avoid mixing Avalonia.Xaml.Interactions.* with Xaml.Behaviors.* in the same app head.
  - Rationale: duplicate assemblies with different identities complicate trimming and packaging.
- Root assemblies for the linker in Release/AOT
  - Add TrimmerRootAssembly entries for Xaml.Behaviors and the Xaml.Behaviors.Interactions.* assemblies used by the app.
  - This prevents the linker from removing assemblies referenced indirectly via XAML.
- Android csproj defaults
  - AndroidMinSdkVersion is 21
  - InvariantGlobalization=true to reduce ICU size and silence globalization warnings

Implementation notes (TestApp.Android)

- The Android head references Xaml.Behaviors.Avalonia and roots the specific Xaml.Behaviors.Interactions.* assemblies for Release/AOT so XmlnsDefinitions resolve behaviors via the Avalonia URI without runtime assembly loads.
- Avoid assembly-qualified XAML namespaces for behaviors; rely on xmlns="https://github.com/avaloniaui" mappings. This keeps APK contents minimal and eliminates FileNotFoundException during startup.
- Note: Rooting the umbrella assembly "Xaml.Behaviors" may fail with IL1032 (not present as a concrete assembly in the meta-package). Keep only the Interactions.* roots in this repo.

Publishing (DotnetDeployer)

- This repo prefers DotnetDeployer for packaging and releases (no NUKE)
- Local usage
  - Install tool (once): dotnet tool install --global DotnetDeployer.Tool
  - Set secrets as env vars in your shell (example)
    - export NUGET_API_KEY={{NUGET_API_KEY}}
  - Dry-run (no push)
    - dotnetdeployer nuget --api-key "$NUGET_API_KEY" --no-push
  - Real publish
    - dotnetdeployer nuget --api-key "$NUGET_API_KEY"
    - Optionally: dotnetdeployer release
- Version bumping
  - DotnetDeployer uses commit message annotations to determine version bumps
  - Add `+semver:major` to the commit message for a major version bump (breaking changes)
  - Add `+semver:minor` to the commit message for a minor version bump (new features)
  - Add `+semver:patch` to the commit message for a patch version bump (bug fixes, default)
  - Example: `git commit -m "Add adaptive dialog sizing +semver:minor"`

Azure Pipelines (CI/CD)

- Location: azure-pipelines.yml
- Triggers: push to master, PRs to master or any branch
- Agent: ubuntu-latest
- Steps summary
  - Full checkout (fetchDepth: 0)
  - Install .NET 9 SDK (UseDotNet@2)
  - Install Android workload (dotnet workload install android)
    - Note: not strictly required for NuGet-only publishing, but kept for optional app release tasks
  - Install DotnetDeployer.Tool globally and print version
  - Package and publish with DotnetDeployer
    - On master: dotnetdeployer nuget --api-key '$(NuGetApiKey)'
    - On other branches: dotnetdeployer nuget --api-key '$(NuGetApiKey)' --no-push (dry run)
- Secrets/variables
  - Variable group: api-keys
  - Variables used: NuGetApiKey (consumed by DotnetDeployer)
- Optional (commented in YAML): release flow using dotnetdeployer release with GitHub token and Android signing inputs

Source generators: Zafiro.Avalonia.Generators

- What changed (rationale)
  - Previously, the Zafiro.Avalonia package embedded and auto-wired the source generator as an Analyzer. This made the generator run implicitly when consuming Zafiro.Avalonia.
  - Now the generator is decoupled and shipped as its own NuGet package (Zafiro.Avalonia.Generators). Consumers opt in explicitly.
- Packaging
  - src/Zafiro.Avalonia.Generators is IsPackable=true
  - The analyzer assembly is included under analyzers/dotnet/cs in the .nupkg
  - A buildTransitive props file (buildTransitive/Zafiro.Avalonia.Generators.props) exposes **/*.axaml as AdditionalFiles automatically in consuming projects
- Consumption guidelines
  - NuGet (recommended for consumers)
    - Add a PackageReference with PrivateAssets=all and IncludeAssets including analyzers and buildTransitive:
      ```xml path=null start=null
      <ItemGroup>
        <PackageReference Include="Zafiro.Avalonia.Generators">
          <PrivateAssets>all</PrivateAssets>
          <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
      </ItemGroup>
      ```
  - Local development inside this repo
    - Projects can reference the generator project as an Analyzer for fast iteration:
      ```xml path=null start=null
      <ItemGroup>
        <ProjectReference Include="..\Zafiro.Avalonia.Generators\Zafiro.Avalonia.Generators.csproj"
                          OutputItemType="Analyzer"
                          ReferenceOutputAssembly="false" />
      </ItemGroup>
      ```
    - The repo uses the UseLocalZafiroReferences switch to toggle between a local ProjectReference to `$(ZafiroSourceRoot)` (true) and NuGet PackageReference (false). The default is false so CI and package builds use published packages.
- Migration note
  - Zafiro.Avalonia no longer includes the generator in its package and no longer references it as an Analyzer. Any project that requires the generated registrations must explicitly reference Zafiro.Avalonia.Generators as shown above.

Local Zafiro development

- This repo should not include Zafiro as a git submodule.
- For local cross-repo iteration, keep Zafiro checked out next to this repo at `../Zafiro`, or set `ZafiroSourceRoot` explicitly.
- Enable local references with `UseLocalZafiroReferences=true`, preferably from an ignored `Directory.Build.local.props`.
- Use `Directory.Build.local.props.example` as the template for that ignored local file.

Coding guidelines (high level)

- Prefer ReactiveUI.Validation and CSharpFunctionalExtensions; lean functional where sensible
- Do not suffix method names with Async even if they return Task
- Keep code, comments, and commit messages in English
- Don't compile the TestApp.Android unless it's necessary, because it slows down the build

Programming style and philosophy

General principles

- Composition over inheritance. Favor small, composable building blocks ("mixins"/extension methods) and keep state minimal.
- Functional core + reactive shell. Keep functions pure when possible and push effects to the edges.
- Reactive-first UI. Prefer representing state as IObservable and binding outputs, instead of imperative event handlers.
- Avoid putting logic in Subscribe. Express pipelines declaratively. If a subscription is unavoidable, centralize side-effects and manage disposables explicitly.
- Source generation for boilerplate. Use ReactiveUI.SourceGenerators where appropriate to reduce manual INotifyPropertyChanged code.

Errors and flow control (CSharpFunctionalExtensions)

- Model outcomes explicitly with Result<T> and Maybe<T>. Avoid exceptions for expected control flow.
- Convert boundary exceptions into Result.Fail with clear, actionable messages.
- Compose with Map/Bind/Tap and provide reactive adapters via the existing mixins (e.g., ReactiveResultMixin) to move between Task/Result/IObservable.
- Use Unit for commands/flows with no payload to keep strong typing in pipelines.

Commands and long-running work

- Represent user intents with commands: ReactiveCommand, EnhancedCommand, and StoppableCommand.
- Drive canExecute from observable state (IObservable<bool>); don’t compute enablement imperatively.
- Represent long-running work with IExecution/Job abstractions. Expose progress and cancellation, surface jobs through JobManager/IStatusBar.
- Return Result<T> (or Unit) from commands to make success/failure explicit and composable.

Filesystem approach (UI consumption of Zafiro.FileSystem)

- Work against IZafiroFileSystem/IZafiroDirectory/IZafiroFile; avoid direct System.IO in UI/domain layers.
- Add capabilities via wrappers: Smart*, Caching*, and ObservableFileSystem for change notifications.
- Compose file operations using Actions (e.g., CopyFileAction) and strategy objects for diffs/comparison (HashCompareStrategy, SizeCompareStrategy).

Navigation, sections, and wizards

- Use Navigator and Sections to model application areas/screens; keep navigation state observable.
- Prefer the SlimWizard builder DSL for wizards; define steps declaratively; keep side-effects behind explicit commands, not subscriptions.

View location and conventions

- Prefer NamingConventionViewLocator to resolve Views for ViewModels by name; avoid manual service location.
- Keep View and ViewModel naming consistent to leverage convention-based resolution.

Misc conventions

- No leading underscores for private fields.
- Methods that return Task should not use the Async suffix.
- Nullable is enabled; avoid null and prefer Maybe<T> to represent absence.
- Prefer records for immutable DTOs and pattern-matching-friendly types where it fits.
- Use the existing logging mixins (e.g., LoggerExtensions/LogMixin) to keep logging consistent.

What makes this library different

- Functional + Reactive philosophy applied consistently across UI, operations, and filesystem consumption.
- First-class execution model (Jobs/Execution) with progress and cancellation wired into the UI.
- Unified, pluggable filesystem with reactive change notifications and composable actions/strategies.
- Declarative wizard DSL and navigation primitives (Navigator/Sections) that keep state observable and testable.
- AOT-friendly XAML behavior strategy for Android (see the section above) that avoids assembly-qualified namespaces and roots required assemblies for trimming.

When implementing new features

- Start from interfaces and pure functions; add behavior via extension methods (mixins) to preserve composability.
- Model user intents as commands; derive canExecute from observable state with WhenAnyValue/WhenAnyChanged.
- Return Result/Maybe and avoid throwing for non-exceptional control flow.
- Keep UI glue reactive: avoid imperative handlers and "Subscribe" logic for business decisions.
- If changes affect Zafiro core, make them in the sibling Zafiro repository and publish that package before publishing Zafiro.Avalonia.

See also

- ../Zafiro/WARP.md for lower-level rules and architecture that apply to the Zafiro core repository used during local development.

Code examples

- Reactive command + Result/Maybe integration (file open flow)

```cs path=/mnt/fast/Repos/SuperJMN-Zafiro/Zafiro.Avalonia/samples/TestApp/TestApp/Samples/Misc/StorageSampleViewModel.cs start=15
public StorageSampleViewModel(IFileSystemPicker storage)
{
    OpenFile = ReactiveCommand.CreateFromTask(async () =>
    {
        Result<Maybe<IFile>> result = await storage.PickForOpen(new FileTypeFilter("All files", new[] { "*.jpg", "*.png", "*.gif", "*.bmp" }));

        return result.Map(maybe => maybe.Map(file => file)).GetValueOrDefault();
    });

    var files = OpenFile.Values().Publish().RefCount();

    SelectedPaths = files.Select(file => file.Name);
    SelectedBytes = files.Select(file => file.Bytes());
}

public IObservable<byte[]> SelectedBytes { get; set; }

public IObservable<string> SelectedPaths { get; }

public ReactiveCommand<Unit, Maybe<IFile>> OpenFile { get; }
```

- Wizard DSL + commands composition

```cs path=/mnt/fast/Repos/SuperJMN-Zafiro/Zafiro.Avalonia/samples/TestApp/TestApp/Samples/SlimWizard/WizardViewModel.cs start=32
ShowWizardInDialog = ReactiveCommand.CreateFromTask(() => CreateWizard().ShowInDialog(dialog, "This is a tasty wizard"));
NavigateToWizard = ReactiveCommand.CreateFromTask(() => CreateWizard().Navigate(navigator));

NavigateToWizard.Merge(ShowWizardInDialog)
    .SelectMany(maybe => ShowResults(maybe).ToSignal())
    .Subscribe()
    .DisposeWith(disposable);
```

```cs path=/mnt/fast/Repos/SuperJMN-Zafiro/Zafiro.Avalonia/samples/TestApp/TestApp/Samples/SlimWizard/WizardViewModel.cs start=57
private static SlimWizard<(int result, string)> CreateWizard()
{
    return WizardBuilder
.StartWith(() => new Page1ViewModel(), "Page 1").NextWith(model => model.ReturnSomeInt.Enhance("Next"))
        .Then(number => new Page2ViewModel(number), "Page 2").NextWhenValid((vm, number) => Result.Success((result: number, vm.Text!)))
        .Then(_ => new Page3ViewModel(), "Completed!").NextWhenValid((_, val) => Result.Success(val), "Close")
        .WithCompletionFinalStep();
}
```

- EnhancedCommand helpers (wrap ReactiveCommand with UX metadata)

```cs path=/mnt/fast/Repos/Zafiro/src/Zafiro.UI/Commands/EnhancedCommand.cs start=7
public static EnhancedCommand<T> Enhance<T>(this IEnhancedCommand<T> enhancedCommand, string? text = null, string? name = null, IObservable<bool>? canExecute = null)
{
    return new EnhancedCommand<T>(ReactiveCommand.CreateFromObservable(enhancedCommand.Execute, canExecute ?? ((IReactiveCommand)enhancedCommand).CanExecute), text ?? enhancedCommand.Text, name ?? enhancedCommand.Name);
}

public static IEnhancedCommand<T, Q> Enhance<T, Q>(this ReactiveCommandBase<T, Q> reactiveCommand, string? text = null, string? name = null)
{
    return new EnhancedCommand<T, Q>(reactiveCommand, text, name);
}
```

- Reactive adapters for Result pipelines

```cs path=/mnt/fast/Repos/Zafiro/src/Zafiro/CSharpFunctionalExtensions/ReactiveResultMixin.cs start=25
public static IObservable<Result<K>> Map<T, K>(this IObservable<Result<T>> observable, Func<T, K> function)
{
    return observable.Select(t => t.Map(function));
}

public static IObservable<Result<K>> Map<T, K>(this IObservable<Result<T>> observable, Func<T, Task<K>> function)
{
    return observable.SelectMany(t => AsyncResultExtensionsRightOperand.Map(t, function));
}
```

- Jobs/Execution with cancellation and progress

```cs path=/mnt/fast/Repos/Zafiro/src/Zafiro.UI/Jobs/Execution/StoppableExecution.cs start=6
public class StoppableExecution : IExecution
{
    public StoppableExecution(IObservable<Unit> observable, IObservable<IProgress> progress, Maybe<IObservable<bool>> canStart)
    {
        Progress = progress;
        var stoppable = StoppableCommand.Create(observable, canStart);
        Start = stoppable.StartReactive;
        Stop = stoppable.StopReactive;
    }

    public ReactiveCommandBase<Unit, Unit> Start { get; }
    public ReactiveCommandBase<Unit, Unit> Stop { get; }
    public IObservable<IProgress> Progress { get; }
}
```

- Filesystem action with progress reporting

```cs path=/mnt/fast/Repos/Zafiro/src/Zafiro.FileSystem.Actions/CopyFileAction.cs start=18
public CopyFileAction(IData source, IMutableFile destination)
{
    Source = source;
    Destination = destination;
    progress = new BehaviorSubject<LongProgress>(new LongProgress(0, source.Length));
}

public IData Source { get; }
public IMutableFile Destination { get; }

public IObservable<LongProgress> Progress => progress.AsObservable();

public async Task<Result> Execute(CancellationToken cancellationToken = default, IScheduler? scheduler = null)
{
    var progressObserver = new Subject<long>();
    using var longProgressSubscription = progressObserver.Select(l => new LongProgress(l, Source.Length)).Subscribe(progress);
    using (new ProgressWatcher(Source, progressObserver))
    {
        var result = await Destination.SetContents(Source, TaskPoolScheduler.Default, cancellationToken).ConfigureAwait(false);
        return result;
    }
}
```

- Navigation ViewModel with reactive wiring and Result/Maybe

```cs path=/mnt/fast/Repos/SuperJMN-Zafiro/Zafiro.Avalonia/src/Zafiro.Avalonia.FileExplorer/Core/Navigator/PathNavigatorViewModel.cs start=16
LoadRequestedPath = ReactiveCommand.CreateFromTask(() => RequestedPath.Map(path => mutableFileSystem.GetDirectory(path).Map(directory => (IRooted<IMutableDirectory>)Rooted.Create(path, directory))));
LoadRequestedPath.HandleErrorsWith(notificationService);
IsNavigating = LoadRequestedPath.IsExecuting;
History = new History();
LoadRequestedPath.Successes().Select(x => x.Path).Select(Maybe.From).BindTo(this, x => x.History.CurrentFolder);
GoBack = History.GoBack;

this.WhenAnyValue(x => x.History.CurrentFolder).Values()
    .Do(path => RequestedPathString = path)
    .ToSignal()
    .InvokeCommand(LoadRequestedPath);

Directories = LoadRequestedPath.Successes().Select(Maybe.From);
Directories
    .Do(maybe => maybe.Execute(x => RequestedPathString = x.Path.ToString()))
    .Subscribe();

currentDirectory = Directories.ToProperty(this, x => x.CurrentDirectory);

RequestedPathString = string.Empty;
GoUp = ReactiveCommand.Create(() => SetAndLoad(CurrentDirectory.Value.Path.Parent().ToString()), Directories.Values().Select(rooted => rooted.Path.Parent().HasValue));
```

- View location by naming convention (+ graceful fallback)

```cs path=/mnt/fast/Repos/SuperJMN-Zafiro/Zafiro.Avalonia/src/Zafiro.Avalonia/ViewLocators/NamingConventionViewLocator.cs start=38
partial void AutoRegister();

public NamingConventionViewLocator Register<TViewModel, TView>()
    where TView : Control, new()
{
    registry[typeof(TViewModel)] = () => new TView();
    return this;
}

public NamingConventionViewLocator Register<TViewModel>(Func<Control> factory)
{
    registry[typeof(TViewModel)] = factory;
    return this;
}
```

```cs path=/mnt/fast/Repos/SuperJMN-Zafiro/Zafiro.Avalonia/src/Zafiro.Avalonia/ViewLocators/NamingConventionViewLocator.cs start=53
private Maybe<Control> TryFromRegistry(object? data)
{
    return Maybe.From(data)
        .Bind(d => registry.TryGetValue(d.GetType(), out var factory)
            ? Maybe.From(factory())
            : Maybe<Control>.None);
}
```

- Binding Maybe<string> to UI

```cs path=/mnt/fast/Repos/SuperJMN-Zafiro/Zafiro.Avalonia/src/Zafiro.Avalonia/Converters/MaybeToStringConverter.cs start=7
public class MaybeToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Maybe<string> maybeValue)
        {
            return maybeValue.HasValue ? maybeValue.Value : string.Empty;
        }
        return string.Empty;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var stringValue = value as string;
        return string.IsNullOrEmpty(stringValue) ? Maybe<string>.None : Maybe<string>.From(stringValue);
    }
}
```
