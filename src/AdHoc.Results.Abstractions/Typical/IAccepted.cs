// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface IAccepted : IResult;

public partial interface ITypedAccepted<TResult>
    : IAccepted, ITypedResult<TResult>
    where TResult : ITypedAccepted<TResult>;
