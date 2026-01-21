// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface ICreated : IResult;

public partial interface ITypedCreated<TResult>
    : ICreated, ITypedResult<TResult>
    where TResult : ITypedCreated<TResult>;

public partial interface ICreated<TValue>
    : ICreated, IResult<TValue>;

public partial interface ITypedCreated<TResult, TValue>
    : ICreated<TValue>, ITypedCreated<TResult>, ITypedResult<TResult, TValue>
    where TResult : ITypedCreated<TResult, TValue>;
