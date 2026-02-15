using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;

namespace AdHoc.Results.SourceGenerators;

[Generator]
public partial class ResultSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext ctx) => ctx
        .RegisterSourceOutput(ctx.ForImplementations([ResultName]).Collect()
        .SelectMany((types, ct) =>
        {
            if (types.IsDefaultOrEmpty)
                return [];

            var resultType = types.First().AllInterfaces
                .First(i => i.ToQualifiedArityName() == ResultName);
            var successProperty = resultType.GetMembers(IsSuccessName)
                .OfType<IPropertySymbol>().First();

            var infos = new Dictionary<INamedTypeSymbol, ResultInfo>(SymbolEqualityComparer.Default);
            foreach (var type in types.OrderBy(t => t.AllInterfaces.Length))
            {
                ct.ThrowIfCancellationRequested();

                var typedResult = type.AllInterfaces
                    .FirstOrDefault(i => i.ToQualifiedArityName() == TypedResultName);
                var typedResults = type.AllInterfaces
                    .FirstOrDefault(i => i.ToQualifiedArityName() == TypedResultsName);
                var typedValueResult = type.AllInterfaces
                    .FirstOrDefault(i => i.ToQualifiedArityName() == TypedValueResultName);
                var info = new ResultInfo(type)
                {
                    HasToBeSealed = type.IsSealed && !type.IsAbstract && (typedResult is not null || typedResults is not null),
                    IsError = type.AllInterfaces.Any(i => i.ToQualifiedArityName() == IErrorName),
                    IsTypedResult = typedResult is not null,
                    TypedValueResult = typedValueResult?.ToQualifiedName(),
                    ValueType = typedValueResult?.TypeArguments[1].ToQualifiedName(),
                };

                var inherits = type.GetTypes().ToImmutableHashSet(SymbolEqualityComparer.Default);
                var others = infos
                    .Where(kv => inherits.Contains(kv.Key))
                    .ToImmutableDictionary(SymbolEqualityComparer.Default);

                info.HasSuccess = !type.RequiresImplementation(
                    successProperty,
                    others.Where(kv => kv.Value.RequireSuccess)
                        .Select(kv => kv.Key).ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default)
                );

                if (typedResult is not null)
                    info.HasTypedSuccess = !type.RequiresImplementation(
                        typedResult.GetMembers(IsSuccessName).OfType<IPropertySymbol>().First(p => p.IsStatic),
                        others.Where(kv => kv.Value.RequireTypedSuccess)
                            .Select(kv => kv.Key).ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default)
                    );

                if (!type.IsAbstract && typedValueResult is not null && type.HasImplemented(
                        typedValueResult.GetMembers("Create").OfType<IMethodSymbol>().First(m => m.IsStatic)))
                    info.HasCreate = true;

                infos[type] = info;
            }

            return infos.Values
                .Where(i => i.RequireImplementation)
                .ToImmutableArray();
        }), static (context, info) =>
        {
            Action<StringBuilder>? appendMembers = null;

            var isSuccessValue = info.IsError ? "false" : "true";
            if (info.RequireSuccess)
                appendMembers += source => source.Append($@"
    bool {ResultName}.{IsSuccessName} => {isSuccessValue};");

            if (info.RequireTypedSuccess)
                appendMembers += source => source.Append($@"
    static bool {TypedResultName}.{IsSuccessName} => {isSuccessValue};");

            if (info.RequireCreate)
                appendMembers += source =>
                {
                    source.Append($@"
    static {info.QualifiedName} {info.TypedValueResult}.Create({info.ValueType} value) => new(value);

    public static implicit operator {info.ValueType}({info.QualifiedName} result) => result.Value;
    public static implicit operator {info.QualifiedName}({info.ValueType} value) => new(value);");
                };

            var source = new StringBuilder()
                .AppendLine("#nullable enable")
                .AppendTypeDeclaration(info, sealing: info.HasToBeSealed, readOnly: info.HasToBeSealed, appendMembers: appendMembers);

            context.AddSource(
                $"{info.QualifiedArityName}.g.cs",
                source.ToString()
            );
        });

    private record ResultInfo : TypeDeclaration
    {
        public bool HasToBeSealed { get; set; }

        public bool IsError { get; set; }
        public bool HasSuccess { get; set; }
        public bool RequireSuccess => !IsError && !HasSuccess;

        public bool IsTypedResult { get; set; }
        public bool HasTypedSuccess { get; set; }
        public bool RequireTypedSuccess => IsTypedResult && !HasTypedSuccess;

        public string? TypedValueResult { get; set; }
        public string? ValueType { get; set; }
        public bool HasCreate { get; set; }
        public bool RequireCreate => !IsAbstract && TypedValueResult is not null && !HasCreate;

        public bool RequireImplementation =>
            HasToBeSealed || RequireSuccess || RequireTypedSuccess || RequireCreate;

        public ResultInfo(INamedTypeSymbol typeSymbol) : base(typeSymbol)
        {
        }
    }
}
