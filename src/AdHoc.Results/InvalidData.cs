// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AdHoc.Results;

public partial record InvalidData : Error, ITypedInvalidData<InvalidData>
{
    public string? Name { get; init; }
    public object? Data { get; init; }
}

public static partial class Result
{
    [StackTraceHidden]
    public static InvalidData InvalidData() => new();

    [StackTraceHidden]
    public static InvalidData InvalidData(string? name, object? data, string? message, Exception? exception, params IEnumerable<IError> errors) => new()
    {
        Message = message,
        Name = name,
        Data = data,
        Exception = exception,
        Errors = [.. errors]
    };

    [StackTraceHidden]
    public static InvalidData InvalidData(string? name, object? data, Exception? exception, params IEnumerable<IError> errors) =>
        InvalidData(name, data, $"Invalid data for '{name}': {data}", exception, errors);

    [StackTraceHidden]
    public static InvalidData InvalidData(Exception? exception, params IEnumerable<Error> errors) =>
        InvalidData(null, null, exception?.Message, exception, errors);

    [StackTraceHidden]
    public static InvalidData InvalidData(string? message, params IEnumerable<Error> errors) =>
        InvalidData(null, null, message, null, errors);

    [StackTraceHidden]
    public static InvalidData InvalidData(
        this object? data,
        string? message = null,
        Exception? exception = null,
        [CallerArgumentExpression(nameof(data))] string? name = null
    ) => InvalidData(name, data, message, exception);
}
