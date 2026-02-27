// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_ASPNET
using System.Reflection;
using System.Runtime.CompilerServices;
using AdHoc.Results.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AdHoc.Results.Abstractions;

public partial interface IResult : IHttpResult, IConvertToActionResult, IEndpointMetadataProvider
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        PopulateMetadata<IResult>(method, builder, 200);
    protected static void PopulateMetadata<TResult>(
        MethodInfo method, EndpointBuilder builder,
        int statusCode
    )
        where TResult : IResult
    =>
        builder.Metadata.Add(new ProducesResultTypeMetadata(
            typeof(TResult), typeof(void), statusCode
        ));
    protected static void PopulateMetadata<TResult>(
        MethodInfo method, EndpointBuilder builder,
        int statusCode, Type valueType
    )
        where TResult : IResult
    =>
        builder.Metadata.Add(new ProducesResultTypeMetadata(
            typeof(TResult), valueType, statusCode, "application/json"
        ));


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => ExecuteAsync(httpContext, 200);
    protected static Task ExecuteAsync(HttpContext httpContext, int statusCode)
    {
        httpContext.Response.StatusCode = statusCode;
        return Task.CompletedTask;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IActionResult IConvertToActionResult.Convert() => Convert(200);
    protected static IActionResult Convert(int statusCode) => new StatusCodeResult(statusCode);
}
#endif
