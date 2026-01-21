// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface INoData : IStatusCodeResult
{
    const int HTTPStatusCode = 204;
}

public partial interface ITypedNoData<TResult>
    : ITypedStatusCodeResult<TResult>;
#endif
