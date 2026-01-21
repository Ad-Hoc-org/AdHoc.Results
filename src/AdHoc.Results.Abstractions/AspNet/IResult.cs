// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_ASPNET
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AdHoc.Results.Abstractions;

public partial interface IResult : IHttpResult, IConvertToActionResult, IEndpointMetadataProvider
{
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        builder.Metadata.Add(new ProducesResponseTypeMetadata(200, typeof(void)));

    Task IHttpResult.ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 200;
        return Task.CompletedTask;
    }

    IActionResult IConvertToActionResult.Convert() => new StatusCodeResult(200);
}

public partial interface IValueResult
{
    IActionResult IConvertToActionResult.Convert() => new ObjectResult(Value)
    {
        DeclaredType = ValueType,
        StatusCode = 200,
        ContentTypes = ["application/json"]
    };

    Task IHttpResult.ExecuteAsync(HttpContext httpContext) =>
        HttpResults.Json(Value, httpContext.RequestServices.GetRequiredService<IOptions<HttpJsonOptions>>().Value.SerializerOptions.GetTypeInfo(ValueType))
            .ExecuteAsync(httpContext);
}

public partial interface ITypedValueResult<TResult>
{
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        builder.Metadata.Add(new ProducesResponseTypeMetadata(200, TResult.ValueType, ["application/json"]));
        builder.FilterFactories.Add((context, next) =>
        {
            var typeInfo = context.ApplicationServices.GetRequiredService<IOptions<HttpJsonOptions>>()
                .Value.SerializerOptions.GetTypeInfo(TResult.ValueType);
            return async invocation =>
            {
                var result = await next(invocation);
                if (result is not TResult valueResult)
                    return result;

                invocation.HttpContext.Response.StatusCode = 200;
                await invocation.HttpContext.Response.WriteAsJsonAsync(
                    valueResult.Value,
                    typeInfo, "application/json",
                    invocation.HttpContext.RequestAborted
                );
                return null;
            };
        });
    }

    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => Task.CompletedTask;
}

#endif
