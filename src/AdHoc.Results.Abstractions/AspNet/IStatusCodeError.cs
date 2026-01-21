// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_ASPNET
using System.Reflection;
using AdHoc.Results.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AdHoc.Results.HTTP.Abstractions;

public partial interface IStatusCodeError
{
    ProblemDetails IError.ToProblemDetails()
    {
        var details = new ProblemDetails()
        {
            Type = Type,
            Detail = Message,
            Status = StatusCode
        };
        if (!Errors.IsDefaultOrEmpty)
            details.Extensions["errors"] = Errors.Select(e => e.ToProblemDetails()).ToArray();
        return details;
    }

    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        builder.Metadata.Add(new ProducesResponseTypeMetadata(500, typeof(ProblemDetails), ["application/problem+json"]));

    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => HttpResults.Problem(
        type: Type,
        detail: Message,
        statusCode: StatusCode,
        extensions: Errors.IsDefaultOrEmpty ? null : new Dictionary<string, object?>
        {
            ["errors"] = Errors.Select(e => e.ToProblemDetails()).ToArray()
        }
    ).ExecuteAsync(httpContext);

    IActionResult IConvertToActionResult.Convert() => new ObjectResult(ToProblemDetails())
    {
        DeclaredType = typeof(ProblemDetails),
        StatusCode = StatusCode,
        ContentTypes = ["application/problem+json"],
    };
}

public partial interface ITypedStatusCodeError<TError>
{
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        builder.Metadata.Add(new ProducesResponseTypeMetadata(TError.HTTPStatusCode, typeof(ProblemDetails), ["application/problem+json"]));
}
#endif
