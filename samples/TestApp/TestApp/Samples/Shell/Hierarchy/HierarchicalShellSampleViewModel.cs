using System;
using Microsoft.Extensions.DependencyInjection;
using Zafiro.UI.Navigation;
using Zafiro.UI.Shell;
using Zafiro.UI.Shell.Utils;
using TestApp.Samples.Shell.Hierarchy.Sections;

namespace TestApp.Samples.Shell.Hierarchy;

[Section(name: "Hierarchical Shell", icon: "mdi-sitemap", sortIndex: 0)]
[SectionGroup("shell", "Shell")]
public sealed class HierarchicalShellSampleViewModel : IDisposable
{
    public const string DemoGroupKey = "hierarchical-shell-demo";

    private readonly ServiceProvider provider;

    public HierarchicalShellSampleViewModel()
    {
        var services = new ServiceCollection();
        services.AddZafiroShell();
        services.AddSectionsFromAttributes(typeof(HierarchicalShellSampleViewModel).Assembly, IsDemoSection);

        provider = services.BuildServiceProvider();
        Shell = provider.GetRequiredService<IHierarchicalShell>();
    }

    public IHierarchicalShell Shell { get; }

    public void Dispose()
    {
        provider.Dispose();
    }

    private static bool IsDemoSection(Type type)
    {
        return type.Namespace == typeof(HierarchicalDemoPage).Namespace;
    }
}
