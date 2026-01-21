// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace AdHoc.Results.Abstractions;

public partial interface ITypedResult<TResult>
    : IResult
    where TResult : ITypedResult<TResult>
{
    static ImmutableArray<Type> IResultVariantsProvider.Variants => [typeof(TResult)];

    static new abstract bool IsSuccess { get; }
    bool IResult.IsSuccess => TResult.IsSuccess;
}
