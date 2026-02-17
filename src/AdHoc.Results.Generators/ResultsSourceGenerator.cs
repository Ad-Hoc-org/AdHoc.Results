using System.Text;
using Microsoft.CodeAnalysis;

namespace AdHoc.Results.Abstractions.Generators;

[Generator]
public partial class ResultsSourceGenerator : IIncrementalGenerator
{
    public const string AbstractionsNamespace = "AdHoc.Results.Abstractions";
    public const string Namespace = "AdHoc.Results";
    public const string GenericResultPrefix = "TResult";
    public const int MinResults = 2;
    public const int MaxResults = 10;

    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        context.RegisterPostInitializationOutput(context =>
        {
            GenerateIResults(context);
            GenerateITypedResults(context);
            GenerateResults(context);
        });

    private static void GenerateIResults(IncrementalGeneratorPostInitializationContext context)
    {
        var source = new StringBuilder();
        source.Append($@"#nullable enable
using System.Collections.Immutable;

#if FEATURE_ASPNET
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
#endif

namespace {AbstractionsNamespace};
");
        for (var results = MinResults; results <= MaxResults; results++)
        {
            source.Append($@"
public partial interface IResults<{GenerateGenericArguments(results, isOut: true)}>
    : IResults");

            AppendGenericConstraints(source, results);

            source.Append($@"
{{

    static ImmutableArray<Type> IResultVariantsProvider.Variants => [
        ");
            source.Append($@"..{GenericResultPrefix}0.Variants");
            for (var i = 1; i < results; i++)
                source.Append($@", ..{GenericResultPrefix}{i}.Variants");
            source.Append($@"
    ];

#if FEATURE_ASPNET
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {{");
            for (var i = 0; i < results; i++)
                source.Append($@"
        {GenericResultPrefix}{i}.PopulateMetadata(method, builder);");
            source.Append($@"
        IResults.PopulateMetadata(method, builder);
    }}
#endif

}}");
        }
        context.AddSource($"IResults.g.cs", source.ToString());
    }


    private static void GenerateITypedResults(IncrementalGeneratorPostInitializationContext context)
    {
        var source = new StringBuilder();
        source.Append($@"#nullable enable
using System.Collections.Immutable;

namespace {AbstractionsNamespace};
");
        for (var results = MinResults; results <= MaxResults; results++)
        {
            var genericArgs = GenerateGenericArguments(results);
            source.Append($@"
public partial interface ITypedResults<TResult, {genericArgs}>
    : ITypedResults<TResult>, IResults<{genericArgs}>
    where TResult : ITypedResults<TResult, {genericArgs}>");

            AppendGenericConstraints(source, results);
            source.AppendLine(";");
        }
        context.AddSource($"ITypedResults.g.cs", source.ToString());
    }

    private static void GenerateResults(IncrementalGeneratorPostInitializationContext context)
    {
        var source = new StringBuilder();
        for (var results = MinResults; results <= MaxResults; results++)
        {
            source.Clear();
            var genericArgs = string.Join(", ", Enumerable.Range(0, results).Select(i => $"{GenericResultPrefix}{i}"));
            var typeDefinition = $"Results<{genericArgs}>";

            source.Append($@"#nullable enable
using System.Collections.Immutable;
using System.Diagnostics;
using {AbstractionsNamespace};

namespace {Namespace};

public partial record struct {typeDefinition}
    : ITypedResults<{typeDefinition}, {genericArgs}>");

            AppendGenericConstraints(source, results);
            source.Append(';');

            context.AddSource($"Results`{results}.g.cs", source.ToString());
        }

        for (var results = MinResults; results <= MaxResults; results++)
        {
            source.Clear();
            var genericArgs = string.Join(", ", Enumerable.Range(0, results).Select(i => $"{GenericResultPrefix}{i}"));
            var typeDefinition = $"Result<TValue, {genericArgs}>";

            source.Append($@"#nullable enable
using System.Collections.Immutable;
using System.Diagnostics;
using {AbstractionsNamespace};

namespace {Namespace};

public partial record struct {typeDefinition}
    : ITypedResults<{typeDefinition}, {genericArgs}>
    where {GenericResultPrefix}0 : ITypedResult<{GenericResultPrefix}0, TValue>");

            AppendGenericConstraints(source, results, start: 1);
            source.Append(';');

            context.AddSource($"Result`{results + 1}.g.cs", source.ToString());
        }
    }


    public static string GenerateGenericArguments(int results, int start = 0, bool isOut = false) =>
        string.Join(", ", Enumerable.Range(start, results - start).Select(i => $"{(isOut ? "out " : "")}{GenericResultPrefix}{i}"));

    public static void AppendGenericConstraints(StringBuilder source, int results, int start = 0, bool typed = false)
    {
        for (var i = start; i < results; i++)
            source.Append($@"
    where {GenericResultPrefix}{i} : {(typed ? $"ITypedResult<{GenericResultPrefix}{i}>" : "IResult")}");
    }
}
