using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Zafiro.Avalonia.Generators;

[Generator]
public class NamingConventionViewLocatorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, static (spc, compilation) =>
        {
            var pairs = FindPairs(compilation, spc);

            var (locatorFqn, locatorNs) = ResolveLocator(compilation, "NamingConventionGeneratedViewLocator");

            var sb = new StringBuilder();
            sb.AppendLine($"namespace {locatorNs};");
            sb.AppendLine();
            sb.AppendLine("file static class NamingConventionGeneratedViewLocator_GlobalRegistrations");
            sb.AppendLine("{");
            sb.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
            sb.AppendLine("    internal static void Initialize()");
            sb.AppendLine("    {");
            foreach (var pair in pairs)
            {
                sb.AppendLine($"        {locatorFqn}.RegisterGlobal<{pair.vm}, {pair.view}>();");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            spc.AddSource("NamingConventionGeneratedViewLocator.GlobalRegistrations.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        });
    }

    private static List<(string vm, string view)> FindPairs(Compilation compilation, SourceProductionContext context)
    {
        var controlType = compilation.GetTypeByMetadataName("Avalonia.Controls.Control");
        if (controlType is null)
        {
            return new List<(string vm, string view)>();
        }

        var pairs = new List<(string vm, string view)>();
        foreach (var (vmSymbol, viewSymbol) in EnumerateCandidates(compilation, controlType, context.CancellationToken))
        {
            var vmName = vmSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var viewName = viewSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            pairs.Add((vmName, viewName));
        }

        return pairs.GroupBy(p => p.vm)
            .Select(group => group.First())
            .ToList();
    }

    private static IEnumerable<(INamedTypeSymbol vm, INamedTypeSymbol view)> EnumerateCandidates(Compilation compilation, INamedTypeSymbol controlType, CancellationToken cancellationToken)
    {
        foreach (var vm in EnumerateAllTypes(compilation.GlobalNamespace, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (vm.TypeKind != TypeKind.Class)
                continue;

            if (!vm.Name.EndsWith("ViewModel", StringComparison.Ordinal))
                continue;

            var ns = vm.ContainingNamespace;
            if (ns is null)
                continue;

            var baseName = vm.Name.Substring(0, vm.Name.Length - "ViewModel".Length);
            var viewCandidates = ns.GetTypeMembers(baseName + "View");
            if (viewCandidates.Length == 0)
                continue;

            foreach (var viewCandidate in viewCandidates)
            {
                if (viewCandidate.TypeKind != TypeKind.Class)
                    continue;

                if (IsDerivedFrom(viewCandidate, controlType))
                {
                    yield return (vm, viewCandidate);
                    break;
                }
            }
        }
    }

    private static bool IsDerivedFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        for (var current = type; current != null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateAllTypes(INamespaceSymbol ns, CancellationToken cancellationToken)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return type;

            foreach (var nested in EnumerateNestedTypes(type, cancellationToken))
                yield return nested;
        }

        foreach (var sub in ns.GetNamespaceMembers())
        {
            foreach (var t in EnumerateAllTypes(sub, cancellationToken))
                yield return t;
        }
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNestedTypes(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return nested;
            foreach (var deeper in EnumerateNestedTypes(nested, cancellationToken))
                yield return deeper;
        }
    }

    private static (string locatorFqn, string locatorNs) ResolveLocator(Compilation compilation, string simpleName)
    {
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
            return ($"global::Zafiro.Avalonia.ViewLocators.{simpleName}", "Zafiro.Avalonia.ViewLocators");
        }

        var fqn = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var ns = symbol.ContainingNamespace?.ToDisplayString() ?? "Zafiro.Avalonia.ViewLocators";
        return (fqn, ns);
    }
}
