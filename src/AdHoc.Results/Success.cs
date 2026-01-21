// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results;

public partial record struct Success : ITypedSuccess<Success>;

public partial record struct Success<TValue>(TValue Value)
    : ITypedSuccess<Success<TValue>, TValue>;


public static partial class Result
{
    public static Success Success() => new();
    public static Success<TValue> Success<TValue>(this TValue value) => new(value);
}
