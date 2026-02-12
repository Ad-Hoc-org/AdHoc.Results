// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_ASPNET
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AdHoc.Results.Abstractions;

public partial interface IError
{
    protected ProblemDetails ToProblemDetails(int statusCode)
    {
        var details = new ProblemDetails()
        {
            Type = Type,
            Detail = Message,
            Status = statusCode
        };
        if (!Errors.IsDefaultOrEmpty)
            details.Extensions["errors"] = Errors.Select(e => e.ToProblemDetails()).ToArray();
        return details;
    }
    ProblemDetails ToProblemDetails() => ToProblemDetails(500);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        PopulateMetadata(method, builder, 500);
    protected static new void PopulateMetadata(MethodInfo method, EndpointBuilder builder, int statusCode) =>
        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode, typeof(ProblemDetails), ["application/problem+json"]));


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => ExecuteAsync(httpContext, this, 500);
    protected static Task ExecuteAsync(HttpContext httpContext, IError error, int statusCode) => TypedResults.Problem(
        type: error.Type,
        detail: error.Message,
        statusCode: statusCode,
        extensions: error.Errors.IsDefaultOrEmpty ? null : new Dictionary<string, object?>
        {
            ["errors"] = error.Errors.Select(e => e.ToProblemDetails()).ToArray()
        }
    ).ExecuteAsync(httpContext);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IActionResult IConvertToActionResult.Convert() => Convert(this, 500);
    protected static IActionResult Convert(IError error, int statusCode) => new ObjectResult(error.ToProblemDetails(statusCode))
    {
        DeclaredType = typeof(ProblemDetails),
        StatusCode = statusCode,
        ContentTypes = ["application/problem+json"],
    };
}
#endif
