// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace AdHoc.Results;

public partial record Forbidden : Error, ITypedForbidden<Forbidden>;

public static partial class Result
{
    [StackTraceHidden]
    public static Forbidden Forbidden() => new();

    [StackTraceHidden]
    public static Forbidden Forbidden(string? message, Exception? exception, params IEnumerable<IError> errors) => new()
    {
        Message = message,
        Exception = exception,
        Errors = [.. errors]
    };

    [StackTraceHidden]
    public static Forbidden Forbidden(Exception? exception, params IEnumerable<IError> errors) =>
        Forbidden(exception?.Message, exception, errors);

    [StackTraceHidden]
    public static Forbidden Forbidden(string? message, params IEnumerable<IError> errors) =>
        Forbidden(message, null, errors);
}
