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

public partial interface IResult : IHttpResult, IConvertToActionResult, IEndpointMetadataProvider
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        PopulateMetadata(method, builder, 200);
    protected static void PopulateMetadata(MethodInfo method, EndpointBuilder builder, int statusCode) =>
        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode, typeof(void)));


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
