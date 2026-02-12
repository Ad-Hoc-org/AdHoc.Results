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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AdHoc.Results.Abstractions;

public partial interface IValueResult
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        PopulateMetadata(method, builder, typeof(object), 200);
    protected static void PopulateMetadata(MethodInfo method, EndpointBuilder builder, Type valueType, int statusCode) =>
        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode, valueType, ["application/json"]));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IActionResult IConvertToActionResult.Convert() => Convert(ValueType, Value, 200);
    protected static IActionResult Convert<TValue>(Type valueType, TValue value, int statusCode) => new ObjectResult(value)
    {
        DeclaredType = valueType,
        StatusCode = statusCode,
        ContentTypes = ["application/json"]
    };


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => ExecuteAsync(httpContext, ValueType, Value, 200);
    protected static Task ExecuteAsync<TValue>(HttpContext httpContext, Type valueType, TValue value, int statusCode) =>
        HttpResults.Json(
            value,
            httpContext.RequestServices.GetRequiredService<IOptions<HttpJsonOptions>>().Value
                .SerializerOptions.GetTypeInfo(valueType),
            statusCode: statusCode
        ).ExecuteAsync(httpContext);
}

public partial interface IResult<out TValue>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        PopulateMetadata<IResult<TValue>, TValue>(method, builder, 200);
    protected static void PopulateMetadata<TResult, TResultValue>(MethodInfo method, EndpointBuilder builder, int statusCode)
        where TResult : IResult<TResultValue>
    {
        PopulateMetadata(method, builder, typeof(TResultValue), statusCode);
        builder.FilterFactories.Add((context, next) =>
        {
            var typeInfo = context.ApplicationServices.GetRequiredService<IOptions<HttpJsonOptions>>()
                .Value.SerializerOptions.GetTypeInfo(typeof(TResultValue));
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
    IActionResult IConvertToActionResult.Convert() => Convert(ValueType, Value, 200);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => Task.CompletedTask;
}

#endif
