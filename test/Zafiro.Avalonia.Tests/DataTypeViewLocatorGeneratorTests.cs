using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Zafiro.Avalonia.Generators;

namespace Zafiro.Avalonia.Tests;

public class DataTypeViewLocatorGeneratorTests
{
    // Minimal Avalonia Control stub so the generator can compile against a "Control" type
    private const string AvaloniaStub = """
        namespace Avalonia.Controls
        {
            public class Control { }
            public class UserControl : Control { }
        }
        """;

    private const string LocatorStub = """
        namespace Zafiro.Avalonia.ViewLocators
        {
            public static class DataTypeViewLocator
            {
                public static void RegisterGlobal<TViewModel, TView>() where TView : global::Avalonia.Controls.Control, new() { }
            }
        }
        """;

    /// <summary>
    /// When x:DataType is an interface implemented by exactly one class the interface
    /// itself should be the registered key (not the concrete class).
    /// </summary>
    [Fact]
    public void Interface_with_single_implementation_registers_interface_not_concrete_type()
    {
        const string csSource = """
            namespace MyApp
            {
                public interface IFooViewModel { }
                public class FooViewModel : IFooViewModel { }
                public class FooView : global::Avalonia.Controls.UserControl { }
            }
            """;

        const string axaml = """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         xmlns:app="clr-namespace:MyApp"
                         x:Class="MyApp.FooView"
                         x:DataType="app:IFooViewModel">
            </UserControl>
            """;

        var generated = RunGenerator(csSource, ("FooView.axaml", axaml));

        Assert.Contains("RegisterGlobal<global::MyApp.IFooViewModel, global::MyApp.FooView>()", generated);
        Assert.DoesNotContain("RegisterGlobal<global::MyApp.FooViewModel,", generated);
    }

    /// <summary>
    /// When x:DataType is an interface implemented by multiple classes the interface
    /// is still registered — ALL implementations can then be resolved at runtime.
    /// </summary>
    [Fact]
    public void Interface_with_multiple_implementations_registers_interface_not_one_concrete_type()
    {
        const string csSource = """
            namespace MyApp
            {
                public interface IFooViewModel { }
                public class FooViewModel : IFooViewModel { }
                public class AnotherFooViewModel : IFooViewModel { }
                public class FooView : global::Avalonia.Controls.UserControl { }
            }
            """;

        const string axaml = """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         xmlns:app="clr-namespace:MyApp"
                         x:Class="MyApp.FooView"
                         x:DataType="app:IFooViewModel">
            </UserControl>
            """;

        var generated = RunGenerator(csSource, ("FooView.axaml", axaml));

        Assert.Contains("RegisterGlobal<global::MyApp.IFooViewModel, global::MyApp.FooView>()", generated);
        Assert.DoesNotContain("RegisterGlobal<global::MyApp.FooViewModel,", generated);
        Assert.DoesNotContain("RegisterGlobal<global::MyApp.AnotherFooViewModel,", generated);
    }

    /// <summary>
    /// When x:DataType is a concrete class (not an interface), that class is registered as-is.
    /// </summary>
    [Fact]
    public void Concrete_viewmodel_registers_concrete_type()
    {
        const string csSource = """
            namespace MyApp
            {
                public class FooViewModel { }
                public class FooView : global::Avalonia.Controls.UserControl { }
            }
            """;

        const string axaml = """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         xmlns:app="clr-namespace:MyApp"
                         x:Class="MyApp.FooView"
                         x:DataType="app:FooViewModel">
            </UserControl>
            """;

        var generated = RunGenerator(csSource, ("FooView.axaml", axaml));

        Assert.Contains("RegisterGlobal<global::MyApp.FooViewModel, global::MyApp.FooView>()", generated);
    }

    // -------------------------------------------------------------------------

    private string RunGenerator(string csSource, params (string fileName, string content)[] axamlFiles)
    {
        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(AvaloniaStub),
            CSharpSyntaxTree.ParseText(LocatorStub),
            CSharpSyntaxTree.ParseText(csSource),
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: syntaxTrees,
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var additionalTexts = axamlFiles
            .Select(f => (Microsoft.CodeAnalysis.AdditionalText)new InMemoryAdditionalText(f.fileName, f.content))
            .ToImmutableArray();

        GeneratorDriver driver = CSharpGeneratorDriver
            .Create(new DataTypeViewLocatorGenerator())
            .AddAdditionalTexts(additionalTexts);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        return driver.GetRunResult()
            .Results
            .Single()
            .GeneratedSources
            .Single(s => s.HintName == "DataTypeViewLocator.GlobalRegistrations.g.cs")
            .SourceText
            .ToString();
    }

    private sealed class InMemoryAdditionalText(string path, string content) : Microsoft.CodeAnalysis.AdditionalText
    {
        public override string Path { get; } = path;
        public override SourceText GetText(System.Threading.CancellationToken cancellationToken = default)
            => SourceText.From(content);
    }
}
