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

public partial interface IResults
{
    protected static new void PopulateMetadata(
        MethodInfo method,
        EndpointBuilder builder
    ) =>
        builder.FilterFactories.Add((context, next) => async invocation =>
        {
            var result = await next(invocation);
            return result is not IResults results ? result : results.Variant;
        });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void IEndpointMetadataProvider.PopulateMetadata(
        MethodInfo method,
        EndpointBuilder builder
    ) =>
        PopulateMetadata(method, builder);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => Variant.ExecuteAsync(httpContext);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IActionResult IConvertToActionResult.Convert() => Variant.Convert();
}
#endif
