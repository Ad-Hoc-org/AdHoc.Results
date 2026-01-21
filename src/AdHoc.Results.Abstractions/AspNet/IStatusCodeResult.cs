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

namespace AdHoc.Results.HTTP.Abstractions;

public partial interface IStatusCodeResult
{
    Task IHttpResult.ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCode;
        return Task.CompletedTask;
    }

    IActionResult IConvertToActionResult.Convert() => new StatusCodeResult(StatusCode);
}

public partial interface IStatusCodeValueResult
{
    Task IHttpResult.ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCode;
        return HttpResults.Json(Value, httpContext.RequestServices.GetRequiredService<IOptions<HttpJsonOptions>>().Value.SerializerOptions.GetTypeInfo(ValueType))
            .ExecuteAsync(httpContext);
    }

    IActionResult IConvertToActionResult.Convert() => new ObjectResult(Value)
    {
        DeclaredType = ValueType,
        StatusCode = StatusCode,
        ContentTypes = ["application/json"]
    };
}

public partial interface ITypedStatusCodeResult<TResult>
{
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        builder.Metadata.Add(new ProducesResponseTypeMetadata(TResult.HTTPStatusCode, typeof(void)));
}

public partial interface ITypedStatusCodeValueResult<TResult>
{
    protected static new void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        builder.Metadata.Add(new ProducesResponseTypeMetadata(TResult.HTTPStatusCode, TResult.ValueType, ["application/json"]));
        builder.FilterFactories.Add((context, next) =>
        {
            var typeInfo = context.ApplicationServices.GetRequiredService<IOptions<HttpJsonOptions>>()
                .Value.SerializerOptions.GetTypeInfo(TResult.ValueType);
            return async invocation =>
            {
                var result = await next(invocation);
                if (result is not TResult valueResult)
                    return result;

                invocation.HttpContext.Response.StatusCode = TResult.HTTPStatusCode;
                await invocation.HttpContext.Response.WriteAsJsonAsync(
                    valueResult.Value,
                    typeInfo, "application/json",
                    invocation.HttpContext.RequestAborted
                );
                return null;
            };
        });
    }

    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        PopulateMetadata(method, builder);

    Task IHttpResult.ExecuteAsync(HttpContext httpContext) => Task.CompletedTask;
}
#endif
