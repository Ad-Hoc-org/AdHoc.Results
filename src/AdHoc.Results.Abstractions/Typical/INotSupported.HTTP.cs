// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface INotSupported : IStatusCodeError
{
    const int HTTPStatusCode = 501;
}

public partial interface ITypedNotSupported<TError>
    : ITypedStatusCodeError<TError>;
#endif
