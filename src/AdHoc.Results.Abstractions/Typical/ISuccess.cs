// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface ISuccess : IResult;

public partial interface ITypedSuccess<TResult>
    : ISuccess, ITypedResult<TResult>
    where TResult : ITypedSuccess<TResult>;

public partial interface ISuccess<TValue>
    : ISuccess, IResult<TValue>;

public partial interface ITypedSuccess<TResult, TValue>
    : ISuccess<TValue>, ITypedSuccess<TResult>, ITypedResult<TResult, TValue>
    where TResult : ITypedSuccess<TResult, TValue>;

