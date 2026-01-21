// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface IUnprocessable : IStatusCodeError
{
    const int HTTPStatusCode = 422;
}

public partial interface ITypedUnprocessable<TError>
    : ITypedStatusCodeError<TError>;
#endif
