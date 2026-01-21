// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AdHoc.Results;

public partial record Unprocessable : Error, ITypedUnprocessable<Unprocessable>;

public static partial class Result
{
    [StackTraceHidden]
    public static Unprocessable Unprocessable() => new();

    [StackTraceHidden]
    public static Unprocessable Unprocessable(string? message, Exception? exception, params IEnumerable<IError> errors) => new()
    {
        Message = message,
        Exception = exception,
        Errors = [.. errors]
    };

    [StackTraceHidden]
    public static Unprocessable Unprocessable(Exception? exception, params IEnumerable<IError> errors) =>
        Unprocessable(exception?.Message, exception, errors);

    [StackTraceHidden]
    public static Unprocessable Unprocessable(string? message, params IEnumerable<IError> errors) =>
        Unprocessable(message, null, errors);

    [StackTraceHidden]
    public static Unprocessable Unprocessable(
        this object? value,
        string? message = null,
        Exception? exception = null,
        [CallerArgumentExpression(nameof(value))] string? name = null
    ) =>
        Unprocessable(message ?? $"{name} is unprocessable: {value}", exception);
}
