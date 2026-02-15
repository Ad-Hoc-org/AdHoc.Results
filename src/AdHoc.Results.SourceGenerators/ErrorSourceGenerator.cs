using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace AdHoc.Results.SourceGenerators;

[Generator]
public partial class ErrorSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext ctx) => ctx
        .RegisterSourceOutput(ctx.ForImplementations([ErrorName]).Collect()
        .SelectMany((types, ct) =>
        {
            if (types.IsDefaultOrEmpty)
                return [];

            var infType = types.First().AllInterfaces
                .First(i => i.ToQualifiedArityName() == IErrorName);
            var typeProperty = infType.GetMembers(ErrorTypeName)
                .OfType<IPropertySymbol>().First();
            var messageProperty = infType.GetMembers(MessageName)
                .OfType<IPropertySymbol>().First();
            var exceptionProperty = infType.GetMembers(ExceptionName)
                .OfType<IPropertySymbol>().First();
            var errorsProperty = infType.GetMembers(ErrorsName)
                .OfType<IPropertySymbol>().First();

            var errorType = types.First().GetTypes()
                .First(t => t.ToQualifiedArityName() == ErrorName);
            var errorTypeProperty = errorType.GetMembers(ErrorTypeName)
                .OfType<IPropertySymbol>().First();

            var infos = new Dictionary<INamedTypeSymbol, ErrorInfo>(SymbolEqualityComparer.Default);
            foreach (var type in types.OrderBy(t => t.AllInterfaces.Length))
            {
                ct.ThrowIfCancellationRequested();
                if (type.IsAbstract)
                    continue;

                var typedError = type.AllInterfaces
                    .FirstOrDefault(i => i.ToQualifiedArityName() == TypedErrorName);
                var info = new ErrorInfo(type)
                {
                    TypedError = typedError?.ToQualifiedName()
                };

                var inherits = type.GetTypes().ToImmutableHashSet(SymbolEqualityComparer.Default);
                var others = infos
                    .Where(kv => inherits.Contains(kv.Key))
                    .ToImmutableDictionary(SymbolEqualityComparer.Default);

                info.HasType = type.HasImplemented(typeProperty) && type.HasImplemented(errorTypeProperty);
                info.HasMessage = type.HasImplemented(messageProperty);
                info.HasException = type.HasImplemented(exceptionProperty);
                info.HasErrors = type.HasImplemented(errorsProperty);

                if (typedError is not null)
                {
                    info.HasErrorType = !type.RequiresImplementation(
                        typedError.GetMembers(TypedErrorTypeName).OfType<IPropertySymbol>().First(),
                        others.Where(o => o.Value.TypedError is not null)
                            .Select(o => o.Key)
                            .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default)
                    );
                    if (!info.HasErrorType)
                        if (type.HasConstant(TypedErrorTypeName, out var constant))
                        {
                            info.ErrorType = "global::" + constant!.ToString();
                        }
                        else
                        {
                            info.ErrorTypeConstant = ToKebabCase(type.Name);
                            info.ErrorType = TypedErrorTypeName;
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

            if (info.RequireErrorType)
            {
                if (info.ErrorTypeConstant is not null)
                    appendMembers += source => source.Append($@"
    public const string {TypedErrorTypeName} = ""{info.ErrorTypeConstant}"";");

                appendMembers += source => source.Append($@"
    static string {info.TypedError}.{TypedErrorTypeName} => {info.ErrorType};");
            }

            if (!info.HasType)
            {
                if (info.TypedError is not null)
                    appendMembers += source => source.Append($@"
    public override string {ErrorTypeName} => {info.ErrorType};");
                else
                    appendMembers += source => source.Append($@"
    public override string? {ErrorTypeName} {{ get; }}");
            }

            if (!info.HasMessage)
                appendMembers += source => source.Append($@"
    public override string? {MessageName} {{ get; init; }}");

            if (!info.HasException)
                appendMembers += source => source.Append($@"
    public override Exception? {ExceptionName} {{ get; init; }}");

            if (!info.HasErrors)
                appendMembers += source => source.Append($@"
    public override global::System.Collections.Immutable.ImmutableArray<global::{IErrorName}> {ErrorsName} {{ get; init; }}");


            var source = new StringBuilder()
                .AppendLine("#nullable enable")
                .AppendTypeDeclaration(info, appendMembers: appendMembers);

            context.AddSource(
                $"{info.QualifiedArityName}.g.cs",
                source.ToString()
            );
        });

    private static string ToKebabCase(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        var sb = new StringBuilder();
        for (int i = 0; i < str.Length; i++)
        {
            var c = str[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                    sb.Append('-');
                sb.Append(char.ToLower(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private record ErrorInfo : TypeDeclaration
    {
        public bool HasType { get; set; }
        public bool HasMessage { get; set; }
        public bool HasException { get; set; }
        public bool HasErrors { get; set; }
        public bool RequireProperties =>
            !HasType || !HasMessage || !HasException || !HasErrors;

        public string? TypedError { get; set; }
        public bool HasErrorType { get; set; }
        public string? ErrorTypeConstant { get; set; }
        public string? ErrorType { get; set; }
        public bool RequireErrorType => TypedError is not null && !HasErrorType;

        public bool RequireImplementation =>
            RequireProperties || RequireErrorType;

        public ErrorInfo(INamedTypeSymbol typeSymbol) : base(typeSymbol)
        {
        }
    }
}
