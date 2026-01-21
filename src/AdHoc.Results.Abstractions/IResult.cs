// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace AdHoc.Results.Abstractions;

public partial interface IResult
    : IResultVariantsProvider
{
    static ImmutableArray<Type> IResultVariantsProvider.Variants => [typeof(IResult)];
    bool IsSuccess { get; }
}
