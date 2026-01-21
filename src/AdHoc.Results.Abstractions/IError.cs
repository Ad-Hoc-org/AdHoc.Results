// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace AdHoc.Results.Abstractions;

public partial interface IError : IResult
{
    static ImmutableArray<Type> IResultVariantsProvider.Variants => [typeof(IError)];

    bool IResult.IsSuccess => false;

    string? Type { get; }
    string? Message { get; }
    Exception? Exception { get; }
    ImmutableArray<IError> Errors { get; }
}
