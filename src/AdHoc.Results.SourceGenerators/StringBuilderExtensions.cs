// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Text;
using Microsoft.CodeAnalysis;

namespace AdHoc.Results.SourceGenerators;

internal static partial class StringBuilderExtensions
{
    public static StringBuilder AppendTypeDeclaration(this StringBuilder source,
        TypeDeclaration type,
        bool partial = true,
        bool sealing = false,
        bool readOnly = false,
        Action<StringBuilder>? appendTypes = null,
        Action<StringBuilder>? appendMembers = null
    )
    {
        if (type.Namespace is not null)
            source.AppendLine($@"namespace {type.Namespace}
{{");

        source.AppendAccessibility(type.Accessibility).Append(' ')
            .AppendTypeKind(type, partial: partial, sealing: sealing, readOnly: readOnly).Append(' ')
            .Append(type.DeclarationName);
        //source.Append(type.ToDisplayString(new SymbolDisplayFormat(
        //    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
        //    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
        //    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        //)));

        if (appendTypes is not null)
            appendTypes(source);

        if (appendMembers is null)
            source.Append(';');
        else
        {
            source.AppendLine().Append('{');
            appendMembers(source);
            source.AppendLine().Append('}');
        }

        if (type.Namespace is not null)
            source.AppendLine().Append('}');

        return source;
    }

    public static StringBuilder AppendAccessibility(this StringBuilder source, Accessibility accessibility) =>
        source.Append(accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "internal"
        });

    public static StringBuilder AppendTypeKind(this StringBuilder source, TypeDeclaration type, bool partial = true, bool sealing = false, bool readOnly = false)
    {
        if (type.IsStatic)
            source.Append("static ");
        else
        {
            if (sealing && type.Kind == TypeKind.Class)
                source.Append("sealed ");
            if (readOnly && type.Kind == TypeKind.Struct)
                source.Append("readonly ");
        }

        if (partial)
            source.Append("partial ");
        if (type.IsRecord)
            source.Append("record ");

        return source.Append(type.Kind.ToString().ToLowerInvariant());
    }

}
