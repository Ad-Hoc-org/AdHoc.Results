// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface IFailure : IStatusCodeError
{
    const int HTTPStatusCode = 500;
}

public partial interface ITypedFailure<TError>
    : ITypedStatusCodeError<TError>;
#endif
