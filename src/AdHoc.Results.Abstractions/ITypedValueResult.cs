// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface ITypedValueResult<TResult>
    : ITypedResult<TResult>, IValueResult
    where TResult : ITypedValueResult<TResult>

{
    static new abstract Type ValueType { get; }
}

public partial interface ITypedResult<TResult, TValue>
    : IResult<TValue>, ITypedValueResult<TResult>
    where TResult : ITypedResult<TResult, TValue>
{
    static Type ITypedValueResult<TResult>.ValueType => typeof(TValue);
    static abstract TResult Create(TValue value);
}
