// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AdHoc.Results;

public partial record NotFound : Error, ITypedNotFound<NotFound>;

public static partial class Result
{
    [StackTraceHidden]
    public static NotFound NotFound() => new();

    [StackTraceHidden]
    public static NotFound NotFound(string? message, Exception? exception, params IEnumerable<IError> errors) => new()
    {
        Message = message,
        Exception = exception,
        Errors = [.. errors]
    };

    [StackTraceHidden]
    public static NotFound NotFound(Exception? exception, params IEnumerable<IError> errors) =>
        NotFound(exception?.Message, exception, errors);

    [StackTraceHidden]
    public static NotFound NotFound(string? message, params IEnumerable<IError> errors) =>
        NotFound(message, exception: null, errors);


    [StackTraceHidden]
    public static NotFound NotFound(
        this object? location,
        Exception? exception = null,
        [CallerArgumentExpression(nameof(location))]
        string? locationExpression = null
    ) =>
        NotFound($"Couldn't find {locationExpression}: {location}", exception: null);

}
