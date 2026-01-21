// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace AdHoc.Results;

public partial record Conflict : Error, ITypedConflict<Conflict>;

public static partial class Result
{
    [StackTraceHidden]
    public static Conflict Conflict() => new();

    [StackTraceHidden]
    public static Conflict Conflict(string? message, Exception? exception, params IEnumerable<IError> errors) => new()
    {
        Message = message,
        Exception = exception,
        Errors = [.. errors]
    };

    [StackTraceHidden]
    public static Conflict Conflict(Exception? exception, params IEnumerable<IError> errors) =>
        Conflict(exception?.Message, exception, errors);

    [StackTraceHidden]
    public static Conflict Conflict(string? message, params IEnumerable<IError> errors) =>
        Conflict(message, null, errors);
}
