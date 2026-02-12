// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;
using System.Runtime.CompilerServices;

#if FEATURE_ASPNET
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
#endif

namespace AdHoc.Results.Abstractions;

public partial interface ISuccess : IStatusCodeResult
{
    const int HTTPStatusCode = 200;
}

public partial interface ITypedSuccess<TResult>
    : ITypedStatusCodeResult<TResult>;

public partial interface ISuccess<TValue>
    : IStatusCodeValueResult;

public partial interface ITypedSuccess<TResult, TValue>
    : ITypedStatusCodeResult<TResult, TValue>
{
#if FEATURE_ASPNET
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        PopulateMetadata<TResult, TValue>(method, builder, TResult.HTTPStatusCode);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => Task.CompletedTask;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IActionResult IConvertToActionResult.Convert() => Convert(TResult.ValueType, Value, TResult.HTTPStatusCode);
#endif
}
#endif
