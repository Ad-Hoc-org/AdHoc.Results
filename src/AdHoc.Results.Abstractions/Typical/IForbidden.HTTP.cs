// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface IForbidden : IStatusCodeError
{
    const int HTTPStatusCode = 403;
}

public partial interface ITypedForbidden<TError>
    : ITypedStatusCodeError<TError>;
#endif
