// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface IValueResult : IResult
{
    Type ValueType { get; }
    object? Value { get; }
}

public partial interface IResult<out TValue> : IValueResult
{
    new TValue Value { get; }

    Type IValueResult.ValueType => typeof(TValue);
    object? IValueResult.Value => Value;
}
