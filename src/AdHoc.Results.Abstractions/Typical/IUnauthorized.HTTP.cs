// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface IUnauthorized : IStatusCodeError
{
    const int HTTPStatusCode = 401;
}

public partial interface ITypedUnauthorized<TError>
    : ITypedStatusCodeError<TError>;

#endif
