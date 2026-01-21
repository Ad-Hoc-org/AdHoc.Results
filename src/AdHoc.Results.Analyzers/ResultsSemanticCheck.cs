// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AdHoc.Results.Analyzers;

internal partial record ResultsSemanticCheck
{
    internal const string _IResultsType = "AdHoc.Results.Abstractions.IResults";
    internal const string _IResultType = "AdHoc.Results.Abstractions.IResult";

    public SyntaxNodeAnalysisContext Context { get; }
    public ImmutableArray<ITypeSymbol> Variants { get; }
    public ArgumentSyntax Argument { get; }
    public HashSet<ITypeSymbol> PossibleTypes { get; }
    private HashSet<SyntaxNode> Synonyms { get; }
    public Predicate<ITypeSymbol> IsPossible { get; set; } = _ => true;

    public ResultsSemanticCheck(
        SyntaxNodeAnalysisContext context,
        ImmutableArray<ITypeSymbol> variants,
        ArgumentSyntax argument,
        ITypeSymbol? argumentType = null
    )
    {
        Context = context;
        Variants = variants;
        Argument = argument;
        PossibleTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        Synonyms = new HashSet<SyntaxNode>(SynonymComparer.Instance);

        if (argumentType is null)
            argumentType = context.SemanticModel.GetTypeInfo(argument.Expression, context.CancellationToken).Type;
        if (argumentType is not null)
            PossibleTypes.Add(argumentType);

        AddSynonym(argument.Expression);
    }


    public bool IsTerminated() =>
        Synonyms.Count == 0 || PossibleTypes.Count == 0;

    public void AddSynonym(SyntaxNode node) =>
        Synonyms.Add(node.WithoutTrivia().WithoutAnnotations());

    public void RemoveSynonym(SyntaxNode node) =>
        Synonyms.Remove(node);

    public void ClearSynonyms() =>
        Synonyms.Clear();

    public void ClearTemporarySynonyms() =>
        Synonyms.RemoveWhere(s => !IsSimpleIdentifier(s));

    public bool HasSynonym(SyntaxNode node) =>
        Synonyms.Contains(node.WithoutTrivia().WithoutAnnotations());

    public bool HasSynonym(SyntaxNode node, out SyntaxNode synonym)
    {
        node = node.WithoutTrivia().WithoutAnnotations();
        synonym = Synonyms.FirstOrDefault(s => SynonymComparer.Instance.Equals(s, node));
        return synonym is not null;
    }


    public void Is(ImmutableArray<ITypeSymbol> types, bool negated)
    {
        if (negated)
            IsNot(types);
        else
            Is(types);
    }

    public void Is(ImmutableArray<ITypeSymbol> types)
    {
        if (types.Length == 0)
            return;

        Predicate<ITypeSymbol> isPossible = t => types.Contains(t, SymbolEqualityComparer.Default);
        var previous = IsPossible;
        IsPossible = t => previous(t) && isPossible(t);

        PossibleTypes.RemoveWhere(t => !isPossible(t));
        foreach (var variant in types)
            if (IsPossible(variant))
                PossibleTypes.Add(variant);
    }

    public void IsNot(ImmutableArray<ITypeSymbol> types)
    {
        if (types.Length == 0)
            return;

        Predicate<ITypeSymbol> isNotPossible = t => types.Contains(t, SymbolEqualityComparer.Default);
        var previous = IsPossible;
        IsPossible = t => previous(t) && !isNotPossible(t);

        PossibleTypes.RemoveWhere(t => isNotPossible(t));
    }

    public bool IsResolved() =>
        PossibleTypes.All(t => Variants.Contains(t, SymbolEqualityComparer.Default));


#if LOGGING
    public void LogDebug(string message)
    {
        Context.ReportDiagnostic(Diagnostic.Create(ResultsConstructorAnalyzer._DebugLog, Argument.GetLocation(), $"{DateTime.Now:O}: {Argument} - {message}"));
    }

    public void LogTypes() =>
        LogDebug("Possible types: " + string.Join(", ", PossibleTypes.Select(t => t.ToDisplayString())));
    public void LogSynonyms() =>
        LogDebug("Synonyms: " + string.Join(", ", Synonyms.Select(s => s.GetType() + " - " + s.WithoutTrivia())));
#endif

    private class SynonymComparer : IEqualityComparer<SyntaxNode>
    {
        public static readonly SynonymComparer Instance = new();
        public bool Equals(SyntaxNode? x, SyntaxNode? y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            return x.ToString() == y.ToString();
        }
        public int GetHashCode(SyntaxNode obj) =>
            obj.ToString().GetHashCode();
    }
}
