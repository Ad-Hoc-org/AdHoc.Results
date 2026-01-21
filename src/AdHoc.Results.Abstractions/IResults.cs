// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace AdHoc.Results.Abstractions;

public partial interface IResults : IResult
{
    IResult Variant { get; }

    bool IResult.IsSuccess => Variant.IsSuccess;
}

public static partial class ResultsExtensions
{
    extension<TResults>(TResults)
        where TResults : IResults
    {
        public static ImmutableArray<Type> Variants => TResults.Variants;
    }
}
