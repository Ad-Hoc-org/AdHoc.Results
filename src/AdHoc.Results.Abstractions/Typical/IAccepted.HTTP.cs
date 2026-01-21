// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface IAccepted : IStatusCodeResult
{
    const int HTTPStatusCode = 202;
}

public partial interface ITypedAccepted<TResult>
    : ITypedStatusCodeResult<TResult>;

#endif
