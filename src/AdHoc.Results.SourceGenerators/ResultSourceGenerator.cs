using System.Collections.Immutable;
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
                var typedValueResult = type.AllInterfaces
                    .FirstOrDefault(i => i.ToQualifiedArityName() == TypedValueResultName);
                var info = new ResultInfo(type)
                {
                    HasToBeSealed = type.IsSealed && !type.IsAbstract && typedResult is not null,
                    IsError = type.AllInterfaces.Any(i => i.ToQualifiedArityName() == IErrorName),
                    TypedResult = typedResult?.ToQualifiedName(),
                    TypedValueResult = typedValueResult?.ToQualifiedName(),
                    ValueType = typedValueResult?.TypeArguments[1].ToQualifiedName(),
                };

                var others = type.GetTypes()
                    .Select(t => infos.TryGetValue(t, out var v) ? v : null!)
                    .Where(v => v is not null)
                    .ToImmutableHashSet();

                info.HasSuccess = type.HasImplemented(successProperty, out var property);
                if (others.Count > 0)
                {
                    if (info.HasSuccess)
                        // has to be implemented in this type, otherwise multiple inheritance
                        info.HasSuccess = property!.ContainingType.TypeKind != TypeKind.Interface
                            || SymbolEqualityComparer.Default.Equals(property!.ContainingType, type);
                    else
                        info.HasSuccess = true; // inherited
                }

                if (typedResult is not null)
                {
                    info.HasTypedSuccess = type.HasImplemented(
                        typedResult.GetMembers(IsSuccessName).OfType<IPropertySymbol>().First(p => p.IsStatic),
                        out property
                    );
                    if (others.Count > 0)
                    {
                        if (info.HasTypedSuccess)
                            // has to be implemented in this type, otherwise multiple inheritance
                            info.HasTypedSuccess = property!.ContainingType.TypeKind != TypeKind.Interface
                                || SymbolEqualityComparer.Default.Equals(property!.ContainingType, type);
                        else
                            info.HasTypedSuccess = others.Any(o => o.HasTypedSuccess); // inherited
                    }
                }

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
    static bool {info.TypedResult}.{IsSuccessName} => {isSuccessValue};");

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

        public string? TypedResult { get; set; }
        public bool HasTypedSuccess { get; set; }
        public bool RequireTypedSuccess => TypedResult is not null && !HasTypedSuccess;

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
