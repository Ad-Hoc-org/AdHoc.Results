// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AdHoc.Results.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ResultsConstructorAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ADHOC_RESULTS_0001";
    private const string _Category = "Usage";
    private static readonly LocalizableString _Title =
        "Constructor argument does not match any variant of IResults<>";

    private static readonly LocalizableString _MessageFormat =
        "The argument type '{0}' does not match any of the variants of '{1}'";

    private static readonly LocalizableString _Description =
        "Variant argument of any constructor of IResults<> types must match one of his variants.";

    internal static readonly DiagnosticDescriptor _Rule = new DiagnosticDescriptor(
        DiagnosticId,
        _Title,
        _MessageFormat,
        _Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: _Description);

#if LOGGING
    internal static readonly DiagnosticDescriptor _DebugLog = new DiagnosticDescriptor(
    "ADHOCRES_DEBUG",
    "Debug Log",
    "{0}",
    "Debug",
    DiagnosticSeverity.Info,
    isEnabledByDefault: true);
#endif

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [_Rule
#if LOGGING
        , _DebugLog
#endif
        ];


    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeCreation, SyntaxKind.ImplicitObjectCreationExpression);
    }

    private static void AnalyzeCreation(
        SyntaxNodeAnalysisContext context
    )
    {
        var objectCreation = (BaseObjectCreationExpressionSyntax)context.Node;
        var typeSymbol = context.SemanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type;
        if (typeSymbol is null)
            return;

        var argumentList = objectCreation.ArgumentList;

        if (argumentList is null || argumentList.Arguments.Count == 0)
            return;

        if (!IsUnsafeResultsConstructor(context, typeSymbol, argumentList, out var resultType, out var resultArgument))
            return;

        var variants = resultType!.TypeArguments;

        var argumentType = context.SemanticModel.GetTypeInfo(resultArgument!.Expression, context.CancellationToken).Type;
        if (argumentType is null)
            return;

        if (variants.Any(v => SymbolEqualityComparer.Default.Equals(argumentType, v)))
            return;
        try
        {
            ResultsSemanticCheck ctx = new(context, variants, resultArgument, argumentType);
            if (ctx.CheckNode(resultArgument.Expression))
                return;
            // skip out of constructor
            var constructor = resultArgument.Parent!.Parent!;
            if (ctx.CheckTree(constructor.Parent, constructor))
                return;
        }
        catch
        {
            // Swallow any exception during analysis to avoid breaking build
        }

        var location = resultArgument!.GetLocation();
        var position = location.SourceSpan.Start;
        context.ReportDiagnostic(Diagnostic.Create(
            _Rule, location,
            argumentType!.ToMinimalDisplayString(context.SemanticModel, position),
            typeSymbol.ToMinimalDisplayString(context.SemanticModel, position)
        ));
    }

    private static bool IsUnsafeResultsConstructor(
        SyntaxNodeAnalysisContext context,
        ITypeSymbol typeSymbol,
        ArgumentListSyntax argumentList,
        out INamedTypeSymbol? resultTypeSymbol,
        out ArgumentSyntax? unsafeResultArgument
    )
    {
        resultTypeSymbol = typeSymbol.AllInterfaces.FirstOrDefault(i => i.ToDisplayString().StartsWith($"{ResultsSemanticCheck._IResultsType}<") && i.IsGenericType);
        if (resultTypeSymbol is null)
        {
            unsafeResultArgument = null;
            return false;
        }

        if (context.SemanticModel.GetSymbolInfo(argumentList.Parent!, context.CancellationToken).Symbol is not IMethodSymbol constructorSymbol)
        {
            unsafeResultArgument = null;
            return false;
        }

        var variants = resultTypeSymbol.TypeArguments;

        for (int i = 0; i < constructorSymbol.Parameters.Length && i < argumentList.Arguments.Count; i++)
        {
            var parameter = constructorSymbol.Parameters[i];
            var parameterType = parameter.Type;

            if (parameterType.ToDisplayString() != ResultsSemanticCheck._IResultType
                || parameterType.AllInterfaces.Any(iface => iface.ToDisplayString() == ResultsSemanticCheck._IResultType))
                continue;

            if (!variants.Any(v => SymbolEqualityComparer.Default.Equals(parameterType, v)))
            {
                unsafeResultArgument = argumentList.Arguments[i];
                return true;
            }
        }

        unsafeResultArgument = null;
        return false;
    }

}
