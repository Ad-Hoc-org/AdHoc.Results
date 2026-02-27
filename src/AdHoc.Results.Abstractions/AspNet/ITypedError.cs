// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_ASPNET
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace AdHoc.Results.Abstractions;

public partial interface ITypedError<TError>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        PopulateMetadata<TError>(method, builder, 500, TError.ErrorType);
}

internal sealed record ErrorTypeMetadata(int StatusCode, string ErrorType);
#endif
