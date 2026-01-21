// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;

namespace AdHoc.Results.SourceGenerators;

internal record TypeDeclaration
{
    public string? Namespace { get; set; }
    public Accessibility Accessibility { get; set; }
    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsRecord { get; set; }
    public TypeKind Kind { get; set; }
    public string QualifiedName { get; set; } = string.Empty;
    public string DeclarationName { get; set; } = string.Empty;
    public string QualifiedArityName { get; set; } = string.Empty;


    public TypeDeclaration() { }

    public TypeDeclaration(INamedTypeSymbol typeSymbol)
    {
        Namespace = typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.ToDisplayString();
        Accessibility = typeSymbol.DeclaredAccessibility;
        IsStatic = typeSymbol.IsStatic;
        IsAbstract = typeSymbol.IsAbstract;
        IsRecord = typeSymbol.IsRecord;
        Kind = typeSymbol.TypeKind;
        DeclarationName = typeSymbol.ToDeclarationName();
        QualifiedName = typeSymbol.ToQualifiedName();
        QualifiedArityName = typeSymbol.ToQualifiedArityName();
    }
}
