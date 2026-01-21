// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AdHoc.Results.SourceGenerators;

internal static partial class IncrementalGeneratorInitializationContextExtensions
{
    public static IncrementalValuesProvider<INamedTypeSymbol> ForImplementations(
        this IncrementalGeneratorInitializationContext context,
        IEnumerable<string> qualitfiedArityName,
        bool partial = true
    ) => context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, ct) => node is TypeDeclarationSyntax,
            transform: (context, ct) =>
            {
                var typeDecl = (TypeDeclarationSyntax)context.Node;

                if (partial && !typeDecl.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)))
                    return default!;

                var symbol = context.SemanticModel.GetDeclaredSymbol(typeDecl);

                if (symbol is not INamedTypeSymbol namedType)
                    return default!;

                if (!namedType.GetTypes().Any(t => qualitfiedArityName.Contains(t.ToQualifiedArityName())))
                    return default!;

                return namedType;
            })
            .Where(static s => s is not null)
            .WithComparer(SymbolEqualityComparer.Default);
}
