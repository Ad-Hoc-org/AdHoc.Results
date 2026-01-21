// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AdHoc.Results.Analyzers;

internal partial record ResultsSemanticCheck
{
    private bool CheckSwitchExpression(
        SwitchExpressionArmSyntax switchArm
    )
    {
        var switchExpression = (SwitchExpressionSyntax)switchArm.Parent!;
        var expression = switchExpression.GoverningExpression;
        var expressionSymbol = Context.SemanticModel.GetSymbolInfo(expression, Context.CancellationToken).Symbol;
        if (expressionSymbol is null)
            return false;

#if LOGGING
        LogDebug("Checking switch arm: " + switchArm.Expression);
#endif
        if (CheckPattern(expression, negated: false, switchArm.Pattern))
            return true;

        foreach (var arm in switchExpression!.Arms)
        {
            if (arm == switchArm)
                break;

#if LOGGING
            LogDebug("Checking other arms: " + arm.Expression);
#endif
            if (CheckPattern(expression, negated: true, arm.Pattern))
                return true;
        }

#if LOGGING
        LogDebug("Checking switch expression");
#endif
        if (CheckNode(expression))
            return true;

        ClearTemporarySynonyms();
        return false;
    }

    private bool CheckSwitchStatement(
        SwitchSectionSyntax switchSection
    )
    {
        var switchStatement = (SwitchStatementSyntax)switchSection.Parent!;
        var expression = switchStatement.Expression;

#if LOGGING
        LogDebug("Checking switch section: " + string.Join(", ", switchSection.Labels));
#endif
        if (switchSection.Labels.Count == 1) // we only support single label sections for now
        {
            var label = switchSection.Labels[0];
            if (label is CasePatternSwitchLabelSyntax pattern)
            {
                if (CheckPattern(expression, negated: false, pattern.Pattern))
                    return true;
            }
            else if (label is CaseSwitchLabelSyntax caseLabel)
            {
                var typeSymbol = Context.SemanticModel.GetTypeInfo(caseLabel.Value, Context.CancellationToken).Type;
                if (typeSymbol is not null && HasSynonym(expression))
                {
                    Is([typeSymbol]);
                    if (IsResolved())
                        return true;
                }
            }
        }

        foreach (var section in switchStatement.Sections)
        {
            if (section == switchSection)
                break;

#if LOGGING
            LogDebug("Checking oposite switch section: " + string.Join(", ", section.Labels));
#endif
            foreach (var label in section.Labels)
                if (label is CasePatternSwitchLabelSyntax casePatternLabel)
                    if (CheckPattern(expression, negated: true, casePatternLabel.Pattern))
                        return true;
        }

#if LOGGING
        LogDebug("Checking switch statement: " + expression);
#endif
        if (CheckNode(expression))
            return true;

        ClearTemporarySynonyms();
        return false;
    }

}
