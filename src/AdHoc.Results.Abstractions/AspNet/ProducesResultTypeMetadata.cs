// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT
#if FEATURE_ASPNET

using Microsoft.AspNetCore.Http.Metadata;

namespace AdHoc.Results.AspNetCore;

public sealed record ProducesResultTypeMetadata(
    Type ResultType,
    Type Type,
    int StatusCode,
    params IEnumerable<string> ContentTypes
)
    : IProducesResponseTypeMetadata
{
    public string? Description { get; init; }
    public string? ErrorType { get; init; }
}
#endif
