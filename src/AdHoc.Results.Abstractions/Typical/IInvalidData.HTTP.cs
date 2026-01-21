// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.HTTP.Abstractions;

namespace AdHoc.Results.Abstractions;

public partial interface IInvalidData : IStatusCodeError
{
    const int HTTPStatusCode = 400;
}

public partial interface ITypedInvalidData<TError>
    : ITypedStatusCodeError<TError>;

#endif
