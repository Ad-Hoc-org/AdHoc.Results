// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

#if FEATURE_ASPNET
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
#endif

namespace AdHoc.Results.Abstractions;

public partial interface ICreated : IStatusCodeResult
{
    const int HTTPStatusCode = 201;
}

public partial interface ITypedCreated<TResult>
    : ITypedStatusCodeResult<TResult>;

public partial interface ICreated<TValue>
    : IStatusCodeValueResult;

public partial interface ITypedCreated<TResult, TValue>
    : ITypedStatusCodeValueResult<TResult>
{
#if FEATURE_ASPNET
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        PopulateMetadata(method, builder);
    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => Task.CompletedTask;
#endif
}
#endif
