// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Xml.Schema;
using Microsoft.CodeAnalysis;

namespace AdHoc.Results.SourceGenerators;

internal static partial class ITypeSymbolExtensions
{
    /// <summary>
    /// this.QualifiedNameOnly = containingSymbol.QualifiedNameOnly + "." + this.Name
    /// </summary>
    public static readonly SymbolDisplayFormat QualifiedNameOnlyFormat =
        new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
        );

    public static readonly SymbolDisplayFormat DeclarationNameFormat =
        new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    public static string ToQualifiedArityName(this INamedTypeSymbol typeSymbol) =>
        typeSymbol.ToDisplayString(QualifiedNameOnlyFormat) + (typeSymbol.Arity == 0 ? "" : $"`{typeSymbol.Arity}");

    public static string ToQualifiedName(this ITypeSymbol typeSymbol) =>
        typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static string ToDeclarationName(this ITypeSymbol typeSymbol) =>
        typeSymbol.ToDisplayString(DeclarationNameFormat);


    public static bool HasImplemented(this ITypeSymbol typeSymbol, IPropertySymbol definedProperty, out IPropertySymbol? property)
    {
        property = GetProperty(typeSymbol, definedProperty);
        if (property is { IsAbstract: false })
            return true;

        foreach (var inf in typeSymbol.Interfaces)
        {
            ImmutableArray<INamedTypeSymbol> interfaces = [inf, .. inf.AllInterfaces];
            foreach (var i in interfaces)
            {
                var p = GetProperty(i, definedProperty);
                if (p is { IsAbstract: false })
                {
                    // two different implementations found
                    if (property is not null && !p.Equals(property, SymbolEqualityComparer.Default))
                    {
                        var propInf = property.ContainingType;
                        if (propInf.AllInterfaces.Contains(i, SymbolEqualityComparer.Default))
                            break;
                        if (i.AllInterfaces.Contains(propInf, SymbolEqualityComparer.Default))
                        {
                            property = p; // more specific
                            break;
                        }
                        return false;
                    }
                    property = p;
                    break;
                }
            }
        }

        return property is not null;

    }
    public static IPropertySymbol? GetProperty(this ITypeSymbol typeSymbol, IPropertySymbol definedProperty) =>
        typeSymbol.GetMembers().OfType<IPropertySymbol>()
            .FirstOrDefault(p => SymbolEqualityComparer.Default.Equals(p, definedProperty)
                || p.ExplicitInterfaceImplementations.Any(e => SymbolEqualityComparer.Default.Equals(e, definedProperty))
            );

    public static bool HasImplemented(this ITypeSymbol typeSymbol, IPropertySymbol definedProperty) =>
        typeSymbol.HasImplemented(definedProperty, out _);

    public static bool RequiresImplementation(
        this ITypeSymbol typeSymbol,
        IPropertySymbol definedProperty,
        IEnumerable<INamedTypeSymbol> willHaveProperty
    )
    {
        if (typeSymbol.GetProperty(definedProperty) is not null)
            return false;

        bool implements = false;
        IPropertySymbol? property = null;
        foreach (var inf in typeSymbol.Interfaces)
        {
            if (willHaveProperty.Contains(inf, SymbolEqualityComparer.Default))
            {
                if (implements)
                    return true;

                implements = true;
                continue;
            }

            var p = GetProperty(inf, definedProperty);
            if (p is { IsAbstract: false })
            {
                if (implements)
                    return true;
        
                implements = true;
                property = p;
                continue;
            }

            foreach (var i in inf.AllInterfaces)
            {
                p = GetProperty(i, definedProperty);
                if (p is { IsAbstract: false })
                {
                    if (property is not null && !p.Equals(property, SymbolEqualityComparer.Default))
                    {
                        var propInf = property.ContainingType;
                        if (propInf.AllInterfaces.Contains(i, SymbolEqualityComparer.Default))
                            break;
                        if (i.AllInterfaces.Contains(propInf, SymbolEqualityComparer.Default))
                        {
                            property = p; // more specific
                            break;
                        }
                        return true;
                    }
                    property = p;
                    break;
                }
            }
        }

        return !(implements || property is not null);
    }


    public static bool HasImplemented(this ITypeSymbol typeSymbol, IMethodSymbol definedMethod, out IMethodSymbol? method)
    {
        method = GetMethod(typeSymbol);
        if (method is { IsAbstract: false })
            return true;

        foreach (var inf in typeSymbol.Interfaces)
        {
            ImmutableArray<INamedTypeSymbol> interfaces = [inf, .. inf.AllInterfaces];
            foreach (var i in interfaces)
            {
                var m = GetMethod(i);
                if (m is { IsAbstract: false })
                {
                    // two different implementations found
                    if (method is not null && !m.Equals(method, SymbolEqualityComparer.Default))
                        return false;
                    method = m;
                    break;
                }
            }
        }

        return method is not null;

        IMethodSymbol? GetMethod(ITypeSymbol typeSymbol) => typeSymbol
            .GetMembers().OfType<IMethodSymbol>()
            .FirstOrDefault(m => SymbolEqualityComparer.Default.Equals(m, definedMethod)
            || m.ExplicitInterfaceImplementations.Any(e => SymbolEqualityComparer.Default.Equals(e, definedMethod)));
    }

    public static bool HasImplemented(this ITypeSymbol typeSymbol, IMethodSymbol definedMethod) =>
        typeSymbol.HasImplemented(definedMethod, out _);


    public static bool HasConstant(this ITypeSymbol typeSymbol, string constantName, out IFieldSymbol? constant)
    {
        constant = GetConstant(typeSymbol);
        if (constant is not null)
            return true;

        var baseType = typeSymbol.BaseType;
        while (baseType is not null)
        {
            constant = GetConstant(baseType);
            if (constant is not null)
                return true;
            baseType = baseType.BaseType;
        }

        foreach (var inf in typeSymbol.Interfaces)
        {
            ImmutableArray<INamedTypeSymbol> interfaces = [inf, .. inf.AllInterfaces];
            foreach (var i in interfaces)
            {
                var c = GetConstant(i);
                if (c is not null && !c.IsAbstract)
                {
                    // two different implementations found
                    if (constant is not null && !c.Equals(constant, SymbolEqualityComparer.Default))
                        return false;
                    constant = c;
                    break;
                }
            }
        }
        return constant is not null;

        IFieldSymbol? GetConstant(ITypeSymbol typeSymbol) => typeSymbol
            .GetMembers().OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.IsConst && f.Name == constantName);
    }

    public static IEnumerable<INamedTypeSymbol> GetTypes(this INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType is not null)
        {
            yield return baseType;
            baseType = baseType.BaseType;
        }

        foreach (var inf in typeSymbol.AllInterfaces)
            yield return inf;
    }

}
