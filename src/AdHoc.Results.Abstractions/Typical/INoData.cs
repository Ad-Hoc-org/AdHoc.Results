// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface INoData : IResult;

public partial interface ITypedNoData<TResult>
    : INoData, ITypedResult<TResult>
    where TResult : ITypedNoData<TResult>;
