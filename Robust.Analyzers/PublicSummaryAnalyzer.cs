#nullable enable
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Robust.Roslyn.Shared;

namespace Robust.Analyzers;

/// <summary>
/// Checks all public methods and Component classes to ensure they have XML summaries.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PublicSummaryAnalyzer : DiagnosticAnalyzer
{
    private const string ObsoleteAttribute = "Robust.Shared.Analyzers.ObsoleteInheritanceAttribute";
    private const string CommandAttribute = "Robust.Shared.Toolshed.CommandImplementationAttribute";
    private const string TestAttribute = "NUnit.Framework.TestAttribute";
    private const string DataFieldAttribute = "Robust.Shared.Serialization.Manager.Attributes.DataFieldAttribute";
    private const string ViewVariablesAttribute = "Robust.Shared.ViewVariables.ViewVariablesAttribute";
    private const string ComponentInterface = "Robust.Shared.GameObjects.IComponent";

    public static readonly DiagnosticDescriptor RuleMethod = new(
        Diagnostics.IdPublicMethodSummaryMissing,
        "Public method is missing XML summary",
        "Public method '{0}' is missing XML summary documentation",
        "Usage",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor RuleComponent = new(
        Diagnostics.IdPublicComponentSummaryMissing,
        "Public component is missing XML summary",
        "Public component '{0}' is missing XML summary documentation",
        "Usage",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor RuleDataField = new(
        Diagnostics.IdPublicDataFieldSummaryMissing,
        "Public DataField is missing XML summary",
        "Public DataField '{0}' is missing XML summary documentation",
        "Usage",
        DiagnosticSeverity.Warning,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RuleMethod, RuleComponent, RuleDataField];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(CheckComponent, SymbolKind.NamedType);
        context.RegisterSymbolAction(CheckDataField, SymbolKind.Field, SymbolKind.Property);
        context.RegisterSyntaxNodeAction(CheckMethods, SyntaxKind.MethodDeclaration);
    }

    private void CheckComponent(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
            return;

        if (typeSymbol.IsValueType || typeSymbol.DeclaredAccessibility != Accessibility.Public)
            return;

        if (!TypeSymbolHelper.ImplementsInterface(typeSymbol, ComponentInterface))
            return;

        var xmlTrivia = typeSymbol.GetDocumentationCommentXml();

        if (xmlTrivia == null || xmlTrivia.Contains("summary") || xmlTrivia.Contains("inheritdoc"))
            return;

        context.ReportDiagnostic(Diagnostic.Create(RuleComponent, context.Symbol.Locations[0], typeSymbol.Name));
    }

    private void CheckDataField(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IFieldSymbol _ && context.Symbol is not IPropertySymbol _)
            return;

        if (context.Symbol.DeclaredAccessibility != Accessibility.Public)
            return;

        if (!AttributeHelper.HasAttribute(context.Symbol, DataFieldAttribute, out _) && !AttributeHelper.HasAttribute(context.Symbol, ViewVariablesAttribute, out _))
            return;

        var xmlTrivia = context.Symbol.GetDocumentationCommentXml();

        if (xmlTrivia == null || xmlTrivia.Contains("summary") || xmlTrivia.Contains("inheritdoc"))
            return;

        context.ReportDiagnostic(Diagnostic.Create(RuleDataField, context.Symbol.Locations[0], context.Symbol.Name));
    }

    private void CheckMethods(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        if (syntaxNodeAnalysisContext.Node is not MethodDeclarationSyntax node)
            return;

        if (!node.Modifiers.Any(SyntaxKind.PublicKeyword))
            return;

        // Need to check overrides / attributes / interface implementations
        var symbol = syntaxNodeAnalysisContext.SemanticModel.GetDeclaredSymbol(node);

        if (symbol == null || symbol.IsOverride)
            return;

        if (AttributeHelper.HasAttribute(symbol, ObsoleteAttribute, out _))
            return;

        if (AttributeHelper.HasAttribute(symbol, CommandAttribute, out _))
            return;

        if (AttributeHelper.HasAttribute(symbol, TestAttribute, out _))
            return;

        foreach (var interfaceSymbol in symbol.ContainingType.AllInterfaces)
        {
            foreach (var interfaceMember in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                var implementation = symbol.ContainingType.FindImplementationForInterfaceMember(interfaceMember);

                if (SymbolEqualityComparer.Default.Equals(implementation, symbol))
                {
                    return;
                }
            }
        }

        var xmlTrivia = symbol.GetDocumentationCommentXml();

        if (xmlTrivia == null || xmlTrivia.Contains("summary") || xmlTrivia.Contains("inheritdoc"))
            return;

        syntaxNodeAnalysisContext.ReportDiagnostic(Diagnostic.Create(RuleMethod, node.Identifier.GetLocation(), node.Identifier.Text));
    }
}
