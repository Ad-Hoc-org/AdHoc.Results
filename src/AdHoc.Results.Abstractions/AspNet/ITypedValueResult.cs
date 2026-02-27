// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_ASPNET
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AdHoc.Results.Abstractions;

public partial interface ITypedValueResult<TResult>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        PopulateMetadata(method, builder, 200);
    protected static void PopulateMetadata(MethodInfo method, EndpointBuilder builder, int statusCode)
    {
        PopulateMetadata<TResult>(method, builder, statusCode, TResult.ValueType);
        builder.FilterFactories.Add((context, next) =>
        {
            var typeInfo = context.ApplicationServices.GetRequiredService<IOptions<HttpJsonOptions>>()
                .Value.SerializerOptions.GetTypeInfo(TResult.ValueType);
            return async invocation =>
            {
                var result = await next(invocation);
                if (result is not TResult valueResult)
                    return result;

                invocation.HttpContext.Response.StatusCode = statusCode;
                await invocation.HttpContext.Response.WriteAsJsonAsync(
                    valueResult.Value,
                    typeInfo, "application/json",
                    invocation.HttpContext.RequestAborted
                );
                return result;
            };
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => Task.CompletedTask;
}

public partial interface ITypedResult<TResult, TValue>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        PopulateMetadata<TResult, TValue>(method, builder, 200);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => Task.CompletedTask;
}

#endif
