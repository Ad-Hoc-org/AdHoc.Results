// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace AdHoc.Results;

public partial record Failure : Error, ITypedFailure<Failure>
{
    public const string ErrorType = "unknown";
}

public static partial class Result
{
    [StackTraceHidden]
    public static Failure Failure() => new();

    [StackTraceHidden]
    public static Failure Failure(string? message, Exception? exception, params IEnumerable<IError> errors) => new()
    {
        Message = message,
        Exception = exception,
        Errors = [.. errors]
    };

    [StackTraceHidden]
    public static Failure Failure(this Exception? exception, params IEnumerable<IError> errors) =>
        Failure(exception?.Message, exception, errors);

    [StackTraceHidden]
    public static Failure Failure(string? message, params IEnumerable<IError> errors) =>
        Failure(message, null, errors);
}
