// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.Abstractions;

namespace AdHoc.Results.HTTP.Abstractions;

public partial interface IStatusCodeResult : IResult
{
    int StatusCode { get; }
}

public partial interface IStatusCodeValueResult : IValueResult, IStatusCodeResult;

public partial interface IStatusCodeError : IError, IStatusCodeResult;
#endif
