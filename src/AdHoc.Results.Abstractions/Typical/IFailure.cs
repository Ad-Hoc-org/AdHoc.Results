// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface IFailure : IError;

public partial interface ITypedFailure<TError>
    : IFailure, ITypedError<TError>
    where TError : ITypedFailure<TError>;
