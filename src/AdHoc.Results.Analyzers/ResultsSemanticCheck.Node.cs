// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AdHoc.Results.Analyzers;

internal partial record ResultsSemanticCheck
{
    public bool CheckTree(
        SyntaxNode? node,
        SyntaxNode? previous
    )
    {
        if (IsResolved())
            return true;
        if (IsTerminated())
            return false;

        while (node is not null)
        {
            if (CheckNode(node)
                || CheckTypeChecks(node, previous)
                || CheckStatements(node, previous)
            )
                return true;

            // no declarations anymore
            if (IsTerminated() || CheckDeclaration(node))
                return false;

            previous = node;
            node = node.Parent;
        }

        return false;
    }

    public bool CheckNode(
        SyntaxNode node
    )
    {
#if LOGGING
        LogDebug("Checking node: " + node.GetType());
#endif
        if (!HasSynonym(node))
            return false;

        return CheckResultsVariantAccess(node);
    }

    private bool CheckTypeChecks(
        SyntaxNode node,
        SyntaxNode? previous
    )
    {
        if (node is SwitchExpressionArmSyntax switchArm)
            return CheckSwitchExpression(switchArm);

        if (node is SwitchSectionSyntax switchSection)
            return CheckSwitchStatement(switchSection);

        if (node is IfStatementSyntax ifStatement)
            return CheckIf(ifStatement, previous);

        if (node is ConditionalExpressionSyntax conditional)
            return CheckConditionalExpression(conditional, previous);

        return false;
    }

    private bool CheckStatements(
        SyntaxNode node,
        SyntaxNode? previous
    )
    {
        if (previous is null)
            return false;
        var statements = node switch
        {
            BlockSyntax block => block.Statements,
            SwitchSectionSyntax switchSection => switchSection.Statements,
            _ => default
        };
        if (statements.Count == 0)
            return false;

        var before = false;
        foreach (var statement in statements.Reverse())
        {
            if (!before)
            {
                before = statement == previous;
                continue;
            }
#if LOGGING
            LogDebug("Checking statement: " + statement.GetType());
#endif
            if (statement is IfStatementSyntax ifStatement)
            {
                var flowAnalysis = Context.SemanticModel.AnalyzeControlFlow(ifStatement.Statement);
                if (flowAnalysis is not null && !flowAnalysis.EndPointIsReachable)
                    if (CheckIf(ifStatement, negated: true))
                        return true;
            }

            if (CheckDeclaration(statement))
                return false;
        }

        return false;
    }
}
