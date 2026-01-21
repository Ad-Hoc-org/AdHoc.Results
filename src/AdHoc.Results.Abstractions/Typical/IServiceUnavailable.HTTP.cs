// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface IServiceUnavailable : IStatusCodeError
{
    const int HTTPStatusCode = 503;
}

public partial interface ITypedServiceUnavailable<TError>
    : ITypedStatusCodeError<TError>;
#endif
