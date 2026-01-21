// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace AdHoc.Results;

public partial record Unauthorized : Error, ITypedUnauthorized<Unauthorized>;

public static partial class Result
{
    [StackTraceHidden]
    public static Unauthorized Unauthorized() => new();

    [StackTraceHidden]
    public static Unauthorized Unauthorized(string? message, Exception? exception, params IEnumerable<IError> errors) => new()
    {
        Message = message,
        Exception = exception,
        Errors = [.. errors]
    };

    [StackTraceHidden]
    public static Unauthorized Unauthorized(Exception? exception, params IEnumerable<IError> errors) =>
        Unauthorized(exception?.Message, exception, errors);

    [StackTraceHidden]
    public static Unauthorized Unauthorized(string? message, params IEnumerable<IError> errors) =>
        Unauthorized(message, null, errors);
}
