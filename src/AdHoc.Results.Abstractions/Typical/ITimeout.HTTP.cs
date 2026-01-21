// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface ITimeout : IStatusCodeError
{
    const int HTTPStatusCode = 408;
}

public partial interface ITypedTimeout<TError>
    : ITypedStatusCodeError<TError>;
#endif
