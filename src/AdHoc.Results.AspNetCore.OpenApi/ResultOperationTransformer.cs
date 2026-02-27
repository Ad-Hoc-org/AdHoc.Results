// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AdHoc.Results.AspNetCore.OpenApi;

internal sealed class ResultOperationTransformer
    : IOpenApiOperationTransformer
{
    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var schemas = context.Document?.Components?.Schemas;
        if (schemas is null)
            return;

        if (!typeof(IResult).IsAssignableFrom(context.Description.ActionDescriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault()?.ReturnType))
            return;

        foreach (var groupByStatusCode in context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IProducesResponseTypeMetadata>()
            .GroupBy(m => m.StatusCode))
        {
            var key = groupByStatusCode.Key.ToString();
            var produces = groupByStatusCode.ToImmutableArray();
            if (!(operation.Responses ??= new())
                    .TryGetValue(key, out var response)
            )
                operation.Responses.TryAdd(key, response = new OpenApiResponse());

            if (response.Content is null)
                continue;

            foreach (var contentType in produces.SelectMany(r => r.ContentTypes).Distinct())
            {
                if (!response!.Content
                        .TryGetValue(contentType, out var content)
                )
                    response.Content.TryAdd(contentType, content = new OpenApiMediaType());

                var oneOf = new List<IOpenApiSchema>();
                IOpenApiSchema? problemSchema = null;
                IList<JsonNode>? errorTypes = null;
                foreach (var produce in produces.Where(r =>
                        (r.Type is not null && r.Type != typeof(void) && !r.ContentTypes.Any())
                        || r.ContentTypes.Contains(contentType)
                ))
                {
                    if (produce.Type is null || produce.Type == typeof(void))
                        continue;

                    IOpenApiSchema schema = await context.GetOrCreateSchemaAsync(
                        produce.Type,
                        cancellationToken: cancellationToken
                    );
                    schema = ResolveSchema(context.Document!, schema);

                    if (produce.Type == typeof(ProblemDetails))
                    {
                        problemSchema ??= schema;

                        if (produce is ProducesResultTypeMetadata { ErrorType: { Length: > 0 } errorType })
                        {
                            if (errorTypes is null)
                            {
                                errorTypes = [];
                                problemSchema = new OpenApiSchema
                                {
                                    AllOf = [
                                        new OpenApiSchema
                                        {
                                            Type = JsonSchemaType.Object,
                                            Properties = new Dictionary<string, IOpenApiSchema>
                                            {
                                                ["type"] = new OpenApiSchema
                                                {
                                                    OneOf = [
                                                        new OpenApiSchema {
                                                            Enum = errorTypes
                                                        },
                                                        new OpenApiSchema {
                                                            Type = JsonSchemaType.String | JsonSchemaType.Null
                                                        }
                                                    ]
                                                }
                                            },
                                        },
                                        problemSchema
                                    ]
                                };
                            }
                            errorTypes.Add(errorType);
                        }

                        continue;
                    }

                    oneOf.Add(schema);
                }

                if (problemSchema is not null)
                    oneOf.Add(problemSchema);
                content.Schema = oneOf.Count == 1 ? oneOf[0] : new OpenApiSchema
                {
                    OneOf = oneOf
                };
            }
        }
    }

    private static IOpenApiSchema ResolveSchema(OpenApiDocument document, IOpenApiSchema openApiSchema)
    {
        if (openApiSchema is OpenApiSchemaReference)
            return openApiSchema;

        if (openApiSchema is not OpenApiSchema schema)
            return openApiSchema;

        if (schema.Metadata?.TryGetValue("x-schema-id", out var metadata) is true &&
            metadata is string { Length: > 0 } id
        )
            return new OpenApiSchemaReference(id, document);

        schema.OneOf = Resolve(schema.OneOf);
        schema.AllOf = Resolve(schema.AllOf);
        schema.AnyOf = Resolve(schema.AnyOf);

        if (schema.Items is not null)
            schema.Items = ResolveSchema(document, schema.Items);

        if (schema.Properties is not null)
        {
            foreach (var key in schema.Properties.Keys)
                schema.Properties[key] = ResolveSchema(document, schema.Properties[key]);
        }

        if (schema.AdditionalProperties is not null)
            schema.AdditionalProperties = ResolveSchema(document, schema.AdditionalProperties);

        return schema;

        IList<IOpenApiSchema>? Resolve(IList<IOpenApiSchema>? schemas) =>
            schemas?.Select(s => ResolveSchema(document, s)).ToList();
    }
}
