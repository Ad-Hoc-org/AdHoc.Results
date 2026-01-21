// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface INotFound : IStatusCodeError
{
    const int HTTPStatusCode = 404;
}

public partial interface ITypedNotFound<TError>
    : ITypedStatusCodeError<TError>;
#endif
