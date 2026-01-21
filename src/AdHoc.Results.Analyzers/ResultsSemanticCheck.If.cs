// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AdHoc.Results.Analyzers;

internal partial record ResultsSemanticCheck
{
    private bool CheckIf(
        IfStatementSyntax ifStatement,
        SyntaxNode? previous
    ) =>
        CheckIf(ifStatement, negated: previous == ifStatement.Else);

    private bool CheckIf(
        IfStatementSyntax ifStatement,
        bool negated = false
    )
    {
#if LOGGING
        LogDebug($"Checking if {(negated ? "not" : "")} -> " + ifStatement.Condition);
#endif

        if (ifStatement.Condition is IsPatternExpressionSyntax pattern)
            return CheckPattern(pattern.Expression, negated, pattern.Pattern);

        ClearTemporarySynonyms();
        return false;
    }

    private bool CheckConditionalExpression(
        ConditionalExpressionSyntax conditional,
        SyntaxNode? previous
    )
    {
        var negated = previous == conditional.WhenFalse;
#if LOGGING
        LogDebug($"Checking conditional {(negated ? "not" : "")} -> " + conditional.Condition);
        LogDebug("Previous node: " + previous);
        LogDebug("WhenFalse node: " + conditional.WhenFalse);
#endif
        if (conditional.Condition is IsPatternExpressionSyntax pattern)
            return CheckPattern(pattern.Expression, negated, pattern.Pattern);

        ClearTemporarySynonyms();
        return false;
    }
}
