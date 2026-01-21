// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results;

public partial record struct Created : ITypedCreated<Created>;

public partial record struct Created<TValue>(TValue Value) : ITypedCreated<Created<TValue>, TValue>;

public static partial class Result
{
    public static Created Created() => new();
    public static Created<TValue> Created<TValue>(this TValue value) => new(value);
}
