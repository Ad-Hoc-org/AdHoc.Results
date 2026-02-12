// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

#if FEATURE_HTTP
using AdHoc.Results.Abstractions;

namespace AdHoc.Results.HTTP.Abstractions;

public partial interface ITypedStatusCodeResult<TResult>
    : ITypedResult<TResult>, IStatusCodeResult
    where TResult : ITypedStatusCodeResult<TResult>
{
    static abstract int HTTPStatusCode { get; }
    int IStatusCodeResult.StatusCode => TResult.HTTPStatusCode;
}

public partial interface ITypedStatusCodeValueResult<TResult>
    : ITypedValueResult<TResult>, ITypedStatusCodeResult<TResult>
    where TResult : ITypedStatusCodeValueResult<TResult>;

public partial interface ITypedStatusCodeResult<TResult, TValue>
    : ITypedResult<TResult, TValue>, ITypedStatusCodeValueResult<TResult>
    where TResult : ITypedStatusCodeResult<TResult, TValue>;

public partial interface ITypedStatusCodeError<TError>
    : ITypedError<TError>, ITypedStatusCodeResult<TError>, IStatusCodeError
    where TError : ITypedStatusCodeError<TError>;

public static partial class TypedStatusCodeResultExtensions
{
    extension<TResult>(TResult)
    where TResult : ITypedStatusCodeResult<TResult>
    {
        public static int HTTPStatusCode => TResult.HTTPStatusCode;
    }
}
#endif
