using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Zafiro.Avalonia.Generators;

[Generator]
public sealed class SectionsRegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, static (spc, compilation) =>
        {
            var sections = FindAnnotatedSections(compilation, spc).OrderBy(s => s.sortIndex).ToList();
            var asm = compilation.AssemblyName ?? "Assembly";
            var safeAsm = MakeSafeForNamespace(asm);
            var assembliesWithRegistrations = GetAssembliesWithRegistrations(compilation, safeAsm).Distinct().OrderBy(x => x).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"namespace Zafiro.UI.Shell.Utils.SectionsGen.{safeAsm};");
            sb.AppendLine();
            sb.AppendLine("public static class GeneratedSectionRegistrations");
            sb.AppendLine("{");
            sb.AppendLine("    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddSectionsFromAttributes(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services, global::Serilog.ILogger? logger = null, global::System.Reactive.Concurrency.IScheduler? scheduler = null)");
            sb.AppendLine("    {");
            sb.AppendLine("        RegisterAnnotatedSections(services);");
            sb.AppendLine("        AddAnnotatedSections(services, logger, scheduler);");
            sb.AppendLine("        return services;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddAllSectionsFromAttributes(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services, global::Serilog.ILogger? logger = null, global::System.Reactive.Concurrency.IScheduler? scheduler = null)");
            sb.AppendLine("    {");
            sb.AppendLine("        EnsureSectionRegistrationsAreRegistered();");
            sb.AppendLine("        return global::Zafiro.UI.Shell.Utils.SectionRegistrationRegistry.AddAllSectionsFromRegistry(services, logger, scheduler);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection RegisterAnnotatedSections(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
            sb.AppendLine("    {");
            foreach (var s in sections)
            {
                if (s.contractFqn == s.implFqn)
                {
                    sb.AppendLine($"        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient(services, typeof({s.implFqn}));");
                }
                else
                {
                    sb.AppendLine($"        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient(services, typeof({s.contractFqn}), typeof({s.implFqn}));");
                }
            }

            sb.AppendLine("        return services;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddAnnotatedSections(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services, global::Serilog.ILogger? logger = null, global::System.Reactive.Concurrency.IScheduler? scheduler = null)");
            sb.AppendLine("    {");
            sb.AppendLine("        global::Zafiro.UI.Navigation.NavigationServiceCollectionExtensions.AddSections(services, builder =>");
            sb.AppendLine("        {");
            foreach (var s in sections)
            {
                var iconSource = s.icon ?? "fa-window-maximize";
                var groupFriendlyName = s.groupFriendlyName; // keep null if null
                var groupName = groupFriendlyName ?? s.groupKey;
                var group = groupName is null
                    ? "null"
                    : $"new global::Zafiro.UI.Navigation.Sections.SectionGroup(\"{Escape(groupName)}\")";
                sb.Append("            builder.AddSection<");
                sb.Append(s.contractFqn);
                sb.Append(">(\"");
                sb.Append(Escape(s.name));
                sb.Append("\", \"");
                sb.Append(Escape(s.friendlyName));
                sb.Append("\", new global::Zafiro.UI.Icon { Source = \"");
                sb.Append(Escape(iconSource));
                sb.Append("\" }, ");
                sb.Append(group);
                sb.Append(", ");
                sb.Append(s.sortIndex);
                sb.Append(");");
                sb.AppendLine();
            }

            sb.AppendLine("        }, logger: logger, scheduler: scheduler ?? global::ReactiveUI.RxApp.MainThreadScheduler);");
            sb.AppendLine("        return services;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    private static void EnsureSectionRegistrationsAreRegistered()");
            sb.AppendLine("    {");
            foreach (var assemblyName in assembliesWithRegistrations)
            {
                sb.AppendLine($"        _ = typeof(global::Zafiro.UI.Shell.Utils.SectionsGen.{assemblyName}.GeneratedSectionRegistrations);");
            }

            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
            sb.AppendLine("    internal static void RegisterSectionsWithRegistry()");
            sb.AppendLine("    {");
            sb.AppendLine($"        global::Zafiro.UI.Shell.Utils.SectionRegistrationRegistry.Register(\"{Escape(asm)}\", static services => RegisterAnnotatedSections(services), static (services, logger, scheduler) => AddAnnotatedSections(services, logger, scheduler));");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            spc.AddSource("GeneratedSectionRegistrations.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));

            var globalUsing = $"global using Zafiro.UI.Shell.Utils.SectionsGen.{safeAsm};";
            spc.AddSource("GeneratedSectionRegistrations.GlobalUsing.g.cs", SourceText.From(globalUsing, Encoding.UTF8));
        });
    }

    private static IEnumerable<(string implFqn, string contractFqn, int sortIndex, string name, string friendlyName, string? icon, string? groupKey, string? groupFriendlyName)> FindAnnotatedSections(Compilation compilation, SourceProductionContext context)
    {
        var attr = compilation.GetTypeByMetadataName("Zafiro.UI.Shell.Utils.SectionAttribute");
        var groupAttr = compilation.GetTypeByMetadataName("Zafiro.UI.Shell.Utils.SectionGroupAttribute");
        if (attr is null)
            yield break;

        foreach (var type in EnumerateAllTypes(compilation.GlobalNamespace, context.CancellationToken))
        {
            if (type.TypeKind != TypeKind.Class)
                continue;

            var sectionAttr = type.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attr));
            if (sectionAttr is null)
                continue;

            string? groupKey = null;
            string? groupFriendlyName = null;

            var sectionGroupAttr = groupAttr is null
                ? null
                : type.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, groupAttr));

            if (sectionGroupAttr is not null)
            {
                var groupCtorArgs = sectionGroupAttr.ConstructorArguments;
                if (groupCtorArgs.Length > 0 && groupCtorArgs[0].Value is string gk)
                {
                    groupKey = gk;
                }

                if (groupCtorArgs.Length > 1 && groupCtorArgs[1].Value is string gf)
                {
                    groupFriendlyName = gf;
                }
            }

            string? icon = null;
            string? explicitName = null;
            string? explicitFriendlyName = null;
            var sortIndex = 0;
            ITypeSymbol? contract = null;

            var sectionCtorArgs = sectionAttr.ConstructorArguments;
            if (sectionCtorArgs.Length >= 1 && sectionCtorArgs[0].Value is string nameStr)
            {
                explicitName = nameStr;
            }

            explicitFriendlyName = GetNamedString(sectionAttr, "FriendlyName");

            if (sectionCtorArgs.Length >= 2 && sectionCtorArgs[1].Value is string iconStr)
            {
                icon = iconStr;
            }

            if (sectionCtorArgs.Length >= 3 && sectionCtorArgs[2].Value is int si)
            {
                sortIndex = si;
            }

            if (sectionCtorArgs.Length >= 4 && sectionCtorArgs[3].Value is ITypeSymbol contractSym)
            {
                contract = contractSym;
            }

            var implFqn = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var contractFqn = (contract ?? type).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            var simple = type.Name;
            var baseName = simple.EndsWith("ViewModel", StringComparison.Ordinal)
                ? simple.Substring(0, simple.Length - "ViewModel".Length)
                : simple;
            var defaultName = ToSpaced(baseName);
            var name = explicitName ?? defaultName;
            var friendlyName = explicitFriendlyName ?? explicitName ?? defaultName;

            yield return (implFqn, contractFqn, sortIndex, name, friendlyName, icon, groupKey, groupFriendlyName);
        }
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

    private static string ToSpaced(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var ch = name[i];
            if (i > 0 && char.IsUpper(ch) && !char.IsWhiteSpace(name[i - 1]))
            {
                sb.Append(' ');
            }

            sb.Append(ch);
        }

        return sb.ToString().TrimStart(' ');
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string? GetNamedString(AttributeData attribute, string propertyName)
    {
        foreach (var arg in attribute.NamedArguments)
        {
            if (arg.Key == propertyName && arg.Value.Value is string s)
            {
                return s;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetAssembliesWithRegistrations(Compilation compilation, string currentAssemblySafeName)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        if (seen.Add(currentAssemblySafeName))
        {
            yield return currentAssemblySafeName;
        }

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol)
            {
                continue;
            }

            var safeName = MakeSafeForNamespace(assemblySymbol.Name);
            if (!seen.Add(safeName))
            {
                continue;
            }

            var metadataName = $"Zafiro.UI.Shell.Utils.SectionsGen.{safeName}.GeneratedSectionRegistrations";
            if (compilation.GetTypeByMetadataName(metadataName) is not null)
            {
                yield return safeName;
            }
        }
    }

    private static string MakeSafeForNamespace(string assemblyName) => new(assemblyName.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
}
