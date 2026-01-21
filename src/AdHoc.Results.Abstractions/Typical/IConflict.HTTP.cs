// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT
#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface IConflict : IStatusCodeError
{
    const int HTTPStatusCode = 409;
}

public partial interface ITypedConflict<TError>
    : ITypedStatusCodeError<TError>;

#endif
