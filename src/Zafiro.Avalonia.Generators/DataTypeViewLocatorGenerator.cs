using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Zafiro.Avalonia.Generators;

[Generator]
public class DataTypeViewLocatorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var axamlFiles = context.AdditionalTextsProvider
            .Where(static text => text.Path.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase))
            .Select((text, token) => (text.Path, Content: text.GetText(token)?.ToString()))
            .Collect();

        var pipeline = context.CompilationProvider.Combine(axamlFiles);

        context.RegisterSourceOutput(pipeline, static (spc, source) =>
        {
            var (compilation, axamls) = source;
            var pairs = FindPairs(compilation, axamls, spc);

            var (locatorFqn, locatorNs) = ResolveLocator(compilation, "DataTypeViewLocator");

            var sb = new StringBuilder();
            sb.AppendLine($"namespace {locatorNs};");
            sb.AppendLine();
            sb.AppendLine("file static class DataTypeViewLocator_GlobalRegistrations");
            sb.AppendLine("{");
            sb.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
            sb.AppendLine("    internal static void Initialize()");
            sb.AppendLine("    {");
            foreach (var pair in pairs)
            {
                sb.AppendLine($"        {locatorFqn}.RegisterGlobal<global::{pair.viewModel}, global::{pair.view}>();");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            spc.AddSource("DataTypeViewLocator.GlobalRegistrations.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        });
    }

    private static List<(string viewModel, string view)> FindPairs(Compilation compilation, ImmutableArray<(string Path, string? Content)> axamls, SourceProductionContext context)
    {
        var rawPairs = new List<(string viewModel, string view)>();

        foreach (var file in axamls)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(file.Content))
            {
                continue;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(file.Content);
            }
            catch
            {
                continue;
            }

            var root = doc.Root;
            if (root is null)
            {
                continue;
            }

            var xNs = root.GetNamespaceOfPrefix("x");
            var classAttr = root.Attribute(xNs + "Class")?.Value;
            var dataTypeAttr = root.Attribute(xNs + "DataType")?.Value;
            if (classAttr is null || dataTypeAttr is null)
            {
                continue;
            }

            var (prefix, typeName) = Split(dataTypeAttr);
            var clrNs = root.Attributes()
                .FirstOrDefault(a => a.IsNamespaceDeclaration && a.Name.LocalName == prefix)?.Value;
            var vmFullNameFromXaml = ToFullName(clrNs, typeName);
            if (vmFullNameFromXaml is null)
            {
                continue;
            }

            var vmSymbol = compilation.GetTypeByMetadataName(vmFullNameFromXaml);
            var chosenVmFullName = vmFullNameFromXaml;

            if (vmSymbol is INamedTypeSymbol named && named.TypeKind == TypeKind.Interface)
            {
                var viewId = GetViewIdentifier(classAttr);

                var impls = EnumerateAllTypes(compilation.GlobalNamespace)
                    .OfType<INamedTypeSymbol>()
                    .Where(t => t.TypeKind == TypeKind.Class && !t.IsAbstract && Implements(t, named))
                    .OrderBy(t => t.ToDisplayString())
                    .ToList();

                if (impls.Count > 0)
                {
                    var matching = impls.FirstOrDefault(t => GetViewModelIdentifier(t.Name).Equals(viewId, StringComparison.Ordinal));

                    var chosen = matching ?? impls.First();
                    chosenVmFullName = ToQualifiedName(chosen);

                    if (impls.Count > 1)
                    {
                        var descriptor = new DiagnosticDescriptor(
                            id: "ZAV0002",
                            title: "Multiple implementations for interface",
                            messageFormat: $"Multiple implementations found for interface {named.ToDisplayString()}. Using {chosenVmFullName} for view {classAttr}",
                            category: "ViewLocation",
                            DiagnosticSeverity.Warning,
                            isEnabledByDefault: true);
                        context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
                    }

                    if (matching is null && impls.Count >= 1)
                    {
                        var descriptorNoMatch = new DiagnosticDescriptor(
                            id: "ZAV0003",
                            title: "No identifier match for interface implementations",
                            messageFormat: $"No implementation matching identifier '{viewId}' for interface {named.ToDisplayString()} and view {classAttr}. Using {chosenVmFullName}",
                            category: "ViewLocation",
                            DiagnosticSeverity.Warning,
                            isEnabledByDefault: true);
                        context.ReportDiagnostic(Diagnostic.Create(descriptorNoMatch, Location.None));
                    }
                }
                else
                {
                    var descriptorNone = new DiagnosticDescriptor(
                        id: "ZAV0004",
                        title: "No implementations found for interface",
                        messageFormat: $"No implementations found for interface {named.ToDisplayString()} referenced by view {classAttr}. Keeping interface mapping.",
                        category: "ViewLocation",
                        DiagnosticSeverity.Warning,
                        isEnabledByDefault: true);
                    context.ReportDiagnostic(Diagnostic.Create(descriptorNone, Location.None));
                }
            }

            rawPairs.Add((chosenVmFullName, classAttr));
        }

        var normalized = new List<(string viewModel, string view)>();
        foreach (var group in rawPairs.GroupBy(p => p.viewModel))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var vmFull = group.Key;
            var vmSimple = vmFull.Split('.').Last();
            var baseName = vmSimple.EndsWith("ViewModel", StringComparison.Ordinal)
                ? vmSimple.Substring(0, vmSimple.Length - "ViewModel".Length)
                : vmSimple;
            var desiredViewSimple = baseName + "View";

            var chosen = group.FirstOrDefault(p => p.view.Split('.').Last() == desiredViewSimple);
            if (string.IsNullOrEmpty(chosen.view))
            {
                chosen = group.First();
            }

            if (group.Skip(1).Any())
            {
                var descriptor = new DiagnosticDescriptor(
                    id: "ZAV0001",
                    title: "Multiple views for view model",
                    messageFormat: $"Multiple views found for {group.Key}. Using {chosen.view}",
                    category: "ViewLocation",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true);
                context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
            }

            normalized.Add(chosen);
        }

        return normalized;
    }

    private static bool Implements(INamedTypeSymbol type, INamedTypeSymbol @interface)
    {
        return type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, @interface));
    }

    private static string GetViewIdentifier(string viewFullName)
    {
        var simple = viewFullName.Split('.').Last();
        return simple.EndsWith("View", StringComparison.Ordinal)
            ? simple.Substring(0, simple.Length - "View".Length)
            : simple;
    }

    private static string GetViewModelIdentifier(string vmSimpleName)
    {
        return vmSimpleName.EndsWith("ViewModel", StringComparison.Ordinal)
            ? vmSimpleName.Substring(0, vmSimpleName.Length - "ViewModel".Length)
            : vmSimpleName;
    }

    private static string ToQualifiedName(INamedTypeSymbol type)
    {
        var parts = new Stack<string>();
        for (var t = type; t is not null; t = t.ContainingType)
        {
            parts.Push(t.Name);
        }

        var ns = type.ContainingNamespace?.ToDisplayString();
        var name = string.Join(".", parts);
        return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateAllTypes(INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            yield return type;

            foreach (var nested in EnumerateNestedTypes(type))
                yield return nested;
        }

        foreach (var sub in ns.GetNamespaceMembers())
        {
            foreach (var t in EnumerateAllTypes(sub))
                yield return t;
        }
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNestedTypes(INamedTypeSymbol type)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            yield return nested;
            foreach (var deeper in EnumerateNestedTypes(nested))
                yield return deeper;
        }
    }

    private static (string prefix, string name) Split(string value)
    {
        var parts = value.Split(':');
        return parts.Length == 2 ? (parts[0], parts[1]) : ("", value);
    }

    private static string? ToFullName(string? clrNamespace, string name)
    {
        if (clrNamespace is null)
        {
            return null;
        }

        var ns = clrNamespace.Split(';').FirstOrDefault()?.Replace("clr-namespace:", "");
        if (ns is null)
        {
            return null;
        }

        return ns + "." + name;
    }

    private static (string locatorFqn, string locatorNs) ResolveLocator(Compilation compilation, string simpleName)
    {
        // Try common namespaces first
        var candidates = new[]
        {
            $"Zafiro.Avalonia.ViewLocators.{simpleName}",
            $"Zafiro.Avalonia.Misc.{simpleName}"
        };

        INamedTypeSymbol? symbol = null;
        foreach (var md in candidates)
        {
            symbol = compilation.GetTypeByMetadataName(md);
            if (symbol is not null) break;
        }

        if (symbol is null)
        {
            // Fallback to default namespace string
            return ($"global::Zafiro.Avalonia.ViewLocators.{simpleName}", "Zafiro.Avalonia.ViewLocators");
        }

        var fqn = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var ns = symbol.ContainingNamespace?.ToDisplayString() ?? "Zafiro.Avalonia.ViewLocators";
        return (fqn, ns);
    }
}
