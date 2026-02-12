// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace AdHoc.Results.Abstractions;

public partial interface ITypedValueResult<TResult>
    : ITypedResult<TResult>, IValueResult
    where TResult : ITypedValueResult<TResult>

{
    static ImmutableArray<Type> IResultVariantsProvider.Variants => [typeof(TResult)];

    static new abstract Type ValueType { get; }
}

public partial interface ITypedResult<TResult, TValue>
    : IResult<TValue>, ITypedValueResult<TResult>
    where TResult : ITypedResult<TResult, TValue>
{
    static ImmutableArray<Type> IResultVariantsProvider.Variants => [typeof(TResult)];
    static Type ITypedValueResult<TResult>.ValueType => typeof(TValue);

    static abstract TResult Create(TValue value);
}
