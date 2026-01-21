using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace AdHoc.Results.SourceGenerators;

[Generator]
public partial class StatusCodeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext ctx) => ctx
        .RegisterImplementationSourceOutput(ctx.ForImplementations([StatusCodeResultName])
        .Collect()
        .SelectMany((types, ct) =>
        {
            if (types.IsDefaultOrEmpty)
                return [];

            var statusCodeInterface = types[0].AllInterfaces
                .First(i => i.ToQualifiedArityName() == StatusCodeResultName);
            var statusCodeProperty = statusCodeInterface
                .GetMembers(StatusCodeName)
                .OfType<IPropertySymbol>().First();
            var infos = new Dictionary<INamedTypeSymbol, StatusCodeResult>(SymbolEqualityComparer.Default);

            foreach (var type in types.OrderBy(t => t.AllInterfaces.Length))
            {
                if (!type.HasConstant(TypedStatusCodeName, out var constant))
                    continue;

                var typedInterface = type.AllInterfaces
                    .FirstOrDefault(i => i.ToQualifiedArityName() == TypedStatusCodeResultName);
                var typedStatusCodeProperty = typedInterface?
                    .GetMembers(TypedStatusCodeName).OfType<IPropertySymbol>().First(p => p.IsStatic);

                var info = new StatusCodeResult(type)
                {
                    Constant = constant!.ToString(),
                    TypedInterfaceName = typedInterface?.ToQualifiedName()
                };

                var others = type.GetTypes()
                    .Select(t => infos.TryGetValue(t, out var v) ? v : null!)
                    .Where(v => v is not null)
                    .ToImmutableHashSet();

                info.HasStatusCode = type.HasImplemented(statusCodeProperty, out var property);
                if (others.Count > 0)
                {
                    if (info.HasStatusCode)
                        // has to be implemented in this type, otherwise multiple inheritance
                        info.HasStatusCode = property!.ContainingType.TypeKind != TypeKind.Interface
                            || SymbolEqualityComparer.Default.Equals(property!.ContainingType, type);
                    else
                        info.HasStatusCode = true; // inherited
                }

                if (typedInterface is not null)
                {
                    info.HasTypedStatusCode = type.HasImplemented(typedStatusCodeProperty!, out property);
                    if (others.Count > 0)
                    {
                        if (typedInterface is not null)
                            if (info.HasTypedStatusCode)
                                // has to be implemented in this type, otherwise multiple inheritance
                                info.HasTypedStatusCode = property!.ContainingType.TypeKind != TypeKind.Interface
                                    || SymbolEqualityComparer.Default.Equals(property!.ContainingType, type);
                            else
                                info.HasTypedStatusCode = others.Any(o => o.HasTypedStatusCode); // inherited
                    }
                }

                if (info.RequireImplementation)
                    infos[type] = info;
            }

            return infos.Values.ToImmutableArray();
        }), static (context, info) =>
        {
            var source = new StringBuilder()
                .AppendLine("#nullable enable")
                .AppendTypeDeclaration(info, appendMembers: source =>
                {
                    if (info.ReqiureStatusCode)
                        source.Append($@"
    int global::{StatusCodeResultName}.{StatusCodeName} => global::{info.Constant};");

                    if (info.RequireTypedStatusCode)
                        source.Append($@"
    static int {info.TypedInterfaceName}.{TypedStatusCodeName} => global::{info.Constant};");
                });

            context.AddSource(
                $"{info.QualifiedArityName}.g.cs",
                source.ToString()
            );
        });


    private record StatusCodeResult : TypeDeclaration
    {
        public string Constant { get; set; } = string.Empty;

        public bool HasStatusCode { get; set; }
        public bool ReqiureStatusCode => !HasStatusCode;

        public string? TypedInterfaceName { get; set; }
        public bool HasTypedStatusCode { get; set; }
        public bool RequireTypedStatusCode => TypedInterfaceName is not null && !HasTypedStatusCode;


        public bool RequireImplementation => ReqiureStatusCode || RequireTypedStatusCode;

        public StatusCodeResult(INamedTypeSymbol typeSymbol) : base(typeSymbol)
        {
        }
    }
}
