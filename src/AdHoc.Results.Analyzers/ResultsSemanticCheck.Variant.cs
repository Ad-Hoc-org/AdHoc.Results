// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AdHoc.Results.Analyzers;

internal partial record ResultsSemanticCheck
{
    private const string VariantName = "Variant";

    private bool CheckResultsVariantAccess(
        SyntaxNode node
    )
    {
        SyntaxNode? resultsExpression;
        switch (node)
        {
            case MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: VariantName
            } memberAccess:
                resultsExpression = memberAccess.Expression;
                break;
            case SubpatternSyntax
            {
                NameColon.Name.Identifier.Text: VariantName
            } subpattern:
                resultsExpression = subpattern.Parent?.Parent;
                if (resultsExpression is RecursivePatternSyntax recursive && recursive.Type is not null)
                {
#if LOGGING
                    LogDebug("Checking recursive variant type: " + recursive.Type);
#endif
                    var recursiveType = Context.SemanticModel.GetTypeInfo(recursive.Type, Context.CancellationToken).Type;
                    if (recursiveType is not null)
                    {
                        var recursiveTypes = ExtractResultsVariants(recursiveType);
                        if (recursiveTypes is not null)
                        {
                            Is(recursiveTypes.Value);
                            if (IsResolved())
                                return true;
                        }
                    }
                }
                break;
            default:
                resultsExpression = null;
                break;
        }
        if (resultsExpression is null)
            return false;

        var typeSymbol = Context.SemanticModel.GetTypeInfo(resultsExpression, Context.CancellationToken).Type;
#if LOGGING
        LogDebug("Checking variant: " + typeSymbol);
#endif
        if (typeSymbol is null)
            return false;

        var types = ExtractResultsVariants(typeSymbol);
        if (types is null)
        {
            return false;
        }

        Is(types.Value);
        return IsResolved();
    }

    private ImmutableArray<ITypeSymbol>? ExtractResultsVariants(
        ITypeSymbol typeSymbol
    )
    {
        var resultTypeSymbol = typeSymbol.AllInterfaces.FirstOrDefault(i => i.ToDisplayString().StartsWith($"{_IResultsType}<") && i.IsGenericType);
#if LOGGING
        LogDebug("Extracting variants from: " + resultTypeSymbol);
#endif
        if (resultTypeSymbol is null)
            return [];

        return resultTypeSymbol.TypeArguments;
    }

}
