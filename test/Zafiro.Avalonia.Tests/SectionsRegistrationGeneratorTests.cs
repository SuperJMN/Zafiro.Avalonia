using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Zafiro.Avalonia.Generators;

namespace Zafiro.Avalonia.Tests;

public class SectionsRegistrationGeneratorTests
{
    [Fact]
    public void Generator_emits_short_name_when_section_attribute_provides_it()
    {
        const string source = """
                              using System;

                              namespace Zafiro.UI.Shell.Utils
                              {
                                  [AttributeUsage(AttributeTargets.Class)]
                                  public class SectionAttribute(string? name = null, string? icon = null, int sortIndex = 0, Type? contractType = null) : Attribute
                                  {
                                      public string? Name { get; } = name;
                                      public string? FriendlyName { get; set; }
                                      public string? ShortName { get; set; }
                                      public string? Icon { get; } = icon;
                                      public int SortIndex { get; } = sortIndex;
                                      public Type? ContractType { get; } = contractType;
                                  }

                                  [AttributeUsage(AttributeTargets.Class)]
                                  public class SectionGroupAttribute(string? key = null, string? friendlyName = null) : Attribute
                                  {
                                      public string? Key { get; } = key;
                                      public string? FriendlyName { get; } = friendlyName;
                                  }
                              }

                              namespace Demo
                              {
                                  [Zafiro.UI.Shell.Utils.Section("users", "mdi-account", 7, ShortName = "USR")]
                                  public class UsersViewModel
                                  {
                                  }
                              }
                              """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            assemblyName: "Demo.Assembly",
            syntaxTrees: [syntaxTree],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SectionsRegistrationGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var generated = driver.GetRunResult()
            .Results
            .Single()
            .GeneratedSources
            .Single(sourceResult => sourceResult.HintName == "GeneratedSectionRegistrations.g.cs")
            .SourceText
            .ToString();

        Assert.Contains("shortName: \"USR\"", generated);
    }
}