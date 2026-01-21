// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AdHoc.Results.Analyzers;

internal partial record ResultsSemanticCheck
{
    private bool CheckDeclaration(
        SyntaxNode node
    )
    {
        if (node is LocalFunctionStatementSyntax or AnonymousFunctionExpressionSyntax or ConstructorDeclarationSyntax)
        {
            ClearSynonyms();
            return true; // we check only inside one function at a time
        }

        if (node is LocalDeclarationStatementSyntax declaration)
        {
            foreach (var variable in declaration.Declaration.Variables)
                foreach (var syn in Synonyms)
                    if (HasRootIdentifier(syn, out var root) &&
                        root.Identifier.Text == variable.Identifier.Text)
                    {
#if LOGGING
                        LogDebug("Removed synonym: " + declaration);
#endif
                        RemoveSynonym(syn);
                        if (IsTerminated())
                            return true;
                        break;
                    }
            return false;
        }

        return false;
    }

    private bool IsSimpleIdentifier(SyntaxNode node) => node switch
    {
        IdentifierNameSyntax name => true,
        MemberAccessExpressionSyntax memberAccess => IsSimpleIdentifier(memberAccess.Expression),
        _ => false
    };

    private bool HasRootIdentifier(SyntaxNode node, out IdentifierNameSyntax root)
    {
        while (node is MemberAccessExpressionSyntax memberAccess)
            node = memberAccess.Expression;
        if (node is IdentifierNameSyntax id)
        {
            root = id;
            return true;
        }
        root = null!;
        return false;
    }
}
