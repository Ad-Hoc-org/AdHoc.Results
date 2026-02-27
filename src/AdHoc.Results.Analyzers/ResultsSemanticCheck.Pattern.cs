// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AdHoc.Results.Analyzers;

internal partial record ResultsSemanticCheck
{

    private bool CheckPattern(
        ExpressionSyntax expression,
        bool negated,
        params ReadOnlySpan<PatternSyntax> patterns
    )
    {
        if (patterns.Length == 0)
            return false;

        ImmutableArray<ITypeSymbol>.Builder types = ImmutableArray.CreateBuilder<ITypeSymbol>();
        var matchedSynonyms = new List<SyntaxNode>();
        try
        {
            foreach (var pattern in patterns)
            {
                if (!negated)
                {
                    var synonym = FindSynoymInPattern(pattern, expression, out var matched, out var from);
                    if (synonym is not null)
                    {
                        // checking recursive variant
                        if (from?.Parent is SubpatternSyntax subpattern && CheckResultsVariantAccess(subpattern))
                            return true;
                        AddSynonym(synonym);
                        matchedSynonyms.Add(matched!);
                    }
                }

                if (pattern is UnaryPatternSyntax { OperatorToken.RawKind: (int)SyntaxKind.NotKeyword } unary)
                {
                    if (negated || patterns.Length > 1)
                        break;
#if LOGGING
                    LogDebug("Visiting not pattern: " + unary.Pattern.GetType());
#endif
                    negated = true;
                    ExtractTypesFromPattern(this, expression, unary.Pattern, negated, types);
                }
                else
                {
                    ExtractTypesFromPattern(this, expression, pattern, negated, types);
                }
            }

#if LOGGING
            LogDebug($"Extracted types {(negated ? "not" : "")} -> " + string.Join(", ", types));
#endif
            Is(types.ToImmutable(), negated);
            return IsResolved();
        }
        finally
        {
            foreach (var synonym in matchedSynonyms)
            {
#if LOGGING
                LogDebug("Remove inner synoym: " + synonym);
#endif
                RemoveSynonym(synonym);
            }
        }


        static ImmutableArray<ITypeSymbol>.Builder ExtractTypesFromPattern(
            ResultsSemanticCheck check,
            ExpressionSyntax expression,
            PatternSyntax pattern,
            bool negated,
            ImmutableArray<ITypeSymbol>.Builder? types = null
        )
        {
#if LOGGING
            check.LogDebug("Extracting pattern: " + pattern.GetType());
#endif
            types ??= ImmutableArray.CreateBuilder<ITypeSymbol>();
            ITypeSymbol? type;
            switch (pattern)
            {
                case ConstantPatternSyntax constant:
                    if (!check.HasSynonym(expression))
                        break;
                    type = check.Context.SemanticModel.GetTypeInfo(constant.Expression, check.Context.CancellationToken).Type;
                    if (type is not null)
                        types.Add(type);
                    break;
                case TypePatternSyntax typePattern:
                    if (!check.HasSynonym(expression))
                        break;
                    type = check.Context.SemanticModel.GetTypeInfo(typePattern.Type, check.Context.CancellationToken).Type;
                    if (type is not null)
                        types.Add(type);
                    break;
                case DeclarationPatternSyntax declaration:
                    if (!check.HasSynonym(expression))
                        break;
                    type = check.Context.SemanticModel.GetTypeInfo(declaration.Type, check.Context.CancellationToken).Type;
                    if (type is not null)
                        types.Add(type);
                    break;
                case BinaryPatternSyntax { OperatorToken.RawKind: (int)SyntaxKind.OrKeyword } orPattern:
                    ExtractTypesFromPattern(check, expression, orPattern.Left, negated, types);
                    ExtractTypesFromPattern(check, expression, orPattern.Right, negated, types);
                    break;
                case RecursivePatternSyntax recursivePattern:
                    VisitSubpatterns(check, recursivePattern, expression, negated, types);
                    static void VisitSubpatterns(
                        ResultsSemanticCheck check,
                        RecursivePatternSyntax recursive,
                        ExpressionSyntax expression,
                        bool negated,
                        ImmutableArray<ITypeSymbol>.Builder types
                    )
                    {
                        if (negated)
                        {
                            static bool IsSupportedNegationPattern(
                                ResultsSemanticCheck check,
                                RecursivePatternSyntax recursive,
                                ExpressionSyntax expression
                            )
                            {
                                if (recursive.PositionalPatternClause is not null)
                                    if (recursive.PositionalPatternClause.Subpatterns.Any(sub => sub.Pattern is not VarPatternSyntax))
                                        return false; // positional patterns are not supported

                                if (recursive.Type is not null && !check.HasSynonym(expression))
                                    return false; // type check with negation is only on synonym supported

                                if (recursive.PropertyPatternClause is not null)
                                    foreach (var subpattern in recursive.PropertyPatternClause.Subpatterns)
                                    {
                                        if (subpattern.NameColon is null)
                                            return false; // unnamed subpatterns are not supported
                                        var propertyExpression = SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            expression.WithoutTrivia().WithoutAnnotations(),
                                            subpattern.NameColon.Name.WithoutTrivia().WithoutAnnotations()
                                        );
                                        if (subpattern.Pattern is RecursivePatternSyntax inner)
                                        {
                                            if (!IsSupportedNegationPattern(check, inner, propertyExpression))
                                                return false;
                                        }
                                        else
                                        {
#if LOGGING
                                            check.LogDebug("Check subpattern for negation support: " + subpattern.GetType());
#endif
                                            if (subpattern.Pattern is not VarPatternSyntax)
                                                return false; // only nonconditional patterns are supported
                                        }
                                    }

                                return true;
                            }

                            if (!IsSupportedNegationPattern(check, recursive, expression))
                                return;
                        }

                        if (recursive.Type is not null && check.HasSynonym(expression))
                        {
                            var type = check.Context.SemanticModel.GetTypeInfo(recursive.Type, check.Context.CancellationToken).Type;
                            if (type is not null)
                                types.Add(type);
                        }

                        if (recursive.PropertyPatternClause is null)
                            return;

                        foreach (var subpattern in recursive.PropertyPatternClause.Subpatterns)
                        {
#if LOGGING
                            check.LogDebug("Visiting subpattern: " + subpattern.GetType());
#endif
                            if (subpattern.NameColon is not null)
                            {
                                var propertyExpression = SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    expression.WithoutTrivia().WithoutAnnotations(),
                                    subpattern.NameColon.Name.WithoutTrivia().WithoutAnnotations()
                                );
                                if (propertyExpression is not null && check.HasSynonym(propertyExpression))
                                    ExtractTypesFromPattern(check, propertyExpression, subpattern.Pattern, negated, types);
                            }
                            if (subpattern.Pattern is RecursivePatternSyntax innerRecursive)
                                VisitSubpatterns(check, innerRecursive, expression, negated, types);
                        }
                    }
                    break;
            }
            return types;
        }
    }

    private ExpressionSyntax? FindSynoymInPattern(
        PatternSyntax pattern,
        ExpressionSyntax expression,
        out SyntaxNode? match,
        out SyntaxNode? from
    )
    {
#if LOGGING
        LogDebug("Looking for synonym in: " + pattern.GetType());
#endif
        var designation = pattern switch
        {
            DeclarationPatternSyntax declaration => declaration.Designation,
            VarPatternSyntax varPattern => varPattern.Designation,
            RecursivePatternSyntax recursive => recursive.Designation,
            _ => null
        };

        if (designation is not null)
        {
            var varName = designation switch
            {
                SingleVariableDesignationSyntax singleVar => SyntaxFactory.IdentifierName(singleVar.Identifier),
                _ => default
            };

            if (varName is not null && HasSynonym(varName, out match))
            {
                from = pattern;
#if LOGGING
                LogDebug("Found synonym: " + expression);
#endif
                return expression;
            }
        }


        if (pattern is RecursivePatternSyntax recursivePattern)
        {
            foreach (var subpattern in recursivePattern.PropertyPatternClause?.Subpatterns ?? [])
            {
                var propertyExpression = subpattern.NameColon is not null
                    ? SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression.WithoutTrivia().WithoutAnnotations(),
                        subpattern.NameColon.Name.WithoutTrivia().WithoutAnnotations()
                    ) : null;

                if (propertyExpression is not null)
                {
                    var result = FindSynoymInPattern(subpattern.Pattern, propertyExpression, out match, out from);
                    if (result is not null)
                        return result;
                }
            }
        }
        from = null;
        match = null;
        return null;
    }
}
