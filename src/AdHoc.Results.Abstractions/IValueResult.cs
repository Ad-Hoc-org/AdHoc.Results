// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace AdHoc.Results.Abstractions;

public partial interface IValueResult : IResult
{
    static ImmutableArray<Type> IResultVariantsProvider.Variants => [typeof(IValueResult)];

    Type ValueType { get; }
    object? Value { get; }
}

public partial interface IResult<out TValue> : IValueResult
{
    static ImmutableArray<Type> IResultVariantsProvider.Variants => [typeof(IResult<TValue>)];

    new TValue Value { get; }

    Type IValueResult.ValueType => typeof(TValue);
    object? IValueResult.Value => Value;
}
