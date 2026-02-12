// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace AdHoc.Results.Abstractions;

public interface ITypedResult
    : IResult
{
    static new abstract bool IsSuccess { get; }
}

public partial interface ITypedResult<TResult>
    : ITypedResult
    where TResult : ITypedResult<TResult>
{
    static ImmutableArray<Type> IResultVariantsProvider.Variants => [typeof(TResult)];
    static bool ITypedResult.IsSuccess => true;
}

public static partial class TypedResultExtensions
{
    extension<TResult>(TResult)
        where TResult : ITypedResult
    {
        public static bool IsSuccess => TResult.IsSuccess;
    }
}
