// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace AdHoc.Results.Abstractions;

public partial interface ITypedError<TError>
    : IError, ITypedResult<TError>
    where TError : ITypedError<TError>
{
    static abstract string ErrorType { get; }
    string IError.Type => TError.ErrorType;

    static ImmutableArray<Type> IResultVariantsProvider.Variants => [typeof(TError)];
    static bool ITypedResult.IsSuccess => false;
}
