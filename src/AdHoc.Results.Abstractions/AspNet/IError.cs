// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_ASPNET
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AdHoc.Results.Abstractions;

public partial interface IError
{
    ProblemDetails ToProblemDetails()
    {
        var details = new ProblemDetails()
        {
            Type = Type,
            Detail = Message,
            Status = 500
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
        statusCode: 500,
        extensions: Errors.IsDefaultOrEmpty ? null : new Dictionary<string, object?>
        {
            ["errors"] = Errors.Select(e => e.ToProblemDetails()).ToArray()
        }
    ).ExecuteAsync(httpContext);

    IActionResult IConvertToActionResult.Convert() => new ObjectResult(ToProblemDetails())
    {
        DeclaredType = typeof(ProblemDetails),
        StatusCode = 500,
        ContentTypes = ["application/problem+json"],
    };
}
#endif
