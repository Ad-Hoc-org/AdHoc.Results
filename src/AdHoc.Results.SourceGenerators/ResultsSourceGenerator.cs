using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;

namespace AdHoc.Results.SourceGenerators;

[Generator]
public partial class ResultsSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext ctx) => ctx
        .RegisterSourceOutput(ctx.ForImplementations([ResultsName]).Collect()
        .SelectMany((types, ct) =>
        {
            if (types.IsDefaultOrEmpty)
                return [];

            var resultType = types.First().AllInterfaces
                .First(i => i.ToQualifiedArityName() == ResultName);
            var resultsType = types.First().AllInterfaces
                .First(i => i.ToQualifiedArityName() == ResultsName);
            var variantProperty = resultsType.GetMembers().OfType<IPropertySymbol>()
                .First(p => p.Name == VariantName);

            var infos = new Dictionary<INamedTypeSymbol, ResultsInfo>(SymbolEqualityComparer.Default);
            foreach (var type in types.OrderBy(t => t.AllInterfaces.Length))
            {
                ct.ThrowIfCancellationRequested();

                if (type.IsAbstract)
                    continue;

                var genericType = type.AllInterfaces
                    .FirstOrDefault(i => i.IsGenericType && i.ToQualifiedNameWithoutArity() == ResultsName);

                var info = new ResultsInfo(type)
                {
                    TypeArguments = genericType?.TypeArguments.Select(t => t.ToQualifiedName()).ToImmutableArray() ?? ImmutableArray<string>.Empty,
                    HasToReadOnly = !type.IsReadOnly,
                    HasVariant = type.GetMembers(variantProperty.Name).OfType<IPropertySymbol>()
                        .Any(p => p.Type.Equals(variantProperty.Type, SymbolEqualityComparer.Default)),
                    HasConstructor = type.HasConstructor(resultType),
                    HasToString = type.GetMembers("ToString").OfType<IMethodSymbol>()
                        .Any(m => m.Parameters.Length == 0 && m.ReturnType.SpecialType == SpecialType.System_String && !m.IsImplicitlyDeclared),
                };

                if (genericType is not null)
                {
                    var missing = new List<string>(genericType.TypeArguments.Length);
                    foreach (var typeArgument in genericType.TypeArguments)
                        if (!type.HasImplicitConversion(typeArgument))
                            missing.Add(typeArgument.ToQualifiedName());
                    info.MissingConversions = missing.ToImmutableArray();

                    var firstResultType = genericType.TypeArguments[0];
                    var valueResultType = firstResultType is ITypeParameterSymbol typeParameter
                        ? typeParameter.ConstraintTypes.OfType<INamedTypeSymbol>()
                            .FirstOrDefault(i => i.ToQualifiedArityName() == TypedValueResultName)
                        : firstResultType.AllInterfaces.FirstOrDefault(i => i.ToQualifiedArityName() == TypedValueResultName);
                    if (valueResultType is not null)
                    {
                        var valueType = valueResultType.TypeArguments[1];
                        info.IsValueResult = true;
                        info.ValueType = valueType.ToQualifiedName();
                        info.HasValueConstructor = type.HasConstructor(valueType);
                        // no conversions from/to interfaces - C# doesn't allow them
                        info.HasValueConversion = valueType is not { TypeKind: TypeKind.Interface } && type.HasImplicitConversion(valueResultType);
                    }
                }

                infos[type] = info;
            }

            return infos.Values
                .Where(i => i.RequireImplementation)
                .ToImmutableArray();
        }), static (context, info) =>
        {
            Action<StringBuilder>? appendMembers = null;
            if (info.RequiresVariant)
                appendMembers += source =>
                    source.Append($@"
    public global::{ResultName} Variant {{ get; }}
");

            if (info.RequiresConstructor)
                if (info.TypeArguments.Length == 0)
                    appendMembers += source =>
                        source.Append($@"
    public {info.Name}(global::{ResultName} result) => Variant = result");
                else
                    appendMembers += source =>
                        source.Append($@"
    public {info.Name}(global::{ResultName} result)
#if DEBUG
    {{
        global::System.Diagnostics.Debug.Assert(result is {string.Join(" or ", info.TypeArguments)}, 
            $""Invalid result type '{{result.GetType()}}' for '{{nameof({info.QualifiedName})}}'"");
        Variant = result;
    }}
#else
    => Variant = result;
#endif
");
            if (info.RequiresValueConstructor)
                appendMembers += source =>
                    source.Append($@"
    public {info.Name}({info.ValueType} value) =>
        Variant = {info.TypeArguments[0]}.Create(value)
");

            if (info.RequiresToString)
                appendMembers += source =>
                    source.Append($@"
    public override string? ToString() => Variant.ToString();
");

            if (info.RequiresConversions)
                appendMembers += source =>
                {
                    foreach (var type in info.MissingConversions)
                        source.Append($@"
    public static implicit operator {info.QualifiedName}({type} result) => new(result);");
                };

            if (info.RequiresValueConversion)
                appendMembers += source =>
                    source.Append($@"
    public static implicit operator {info.QualifiedName}({info.ValueType} value) => new(value);");

            var source = new StringBuilder()
                .AppendLine("#nullable enable")
                .AppendTypeDeclaration(info, readOnly: true, appendMembers: appendMembers);

            context.AddSource(
                $"{info.QualifiedArityName}.g.cs",
                source.ToString()
            );
        });

    private record ResultsInfo : TypeDeclaration
    {
        public bool HasToReadOnly { get; set; }

        public bool HasConstructor { get; set; }
        public ImmutableArray<string> TypeArguments { get; set; }
        public bool RequiresConstructor => !HasConstructor;

        public bool HasVariant { get; set; }
        public bool RequiresVariant => !HasVariant;

        public bool HasToString { get; set; }
        public bool RequiresToString => !HasToString;

        public ImmutableArray<string> MissingConversions { get; set; }
        public bool RequiresConversions => MissingConversions.Length > 0;


        public bool IsValueResult { get; set; }
        public string? ValueType { get; set; }

        public bool HasValueConstructor { get; set; }
        public bool RequiresValueConstructor => IsValueResult && !HasValueConstructor;

        public bool HasValueConversion { get; set; }
        public bool RequiresValueConversion => IsValueResult && !HasValueConversion;


        public bool RequireImplementation =>
            HasToReadOnly || RequiresToString || RequiresConstructor || RequiresVariant || RequiresConversions || RequiresValueConversion || RequiresValueConstructor;

        public ResultsInfo(INamedTypeSymbol typeSymbol) : base(typeSymbol)
        {
        }
    }
}
