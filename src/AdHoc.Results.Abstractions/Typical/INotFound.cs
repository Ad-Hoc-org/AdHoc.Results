// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface INotFound : IError;

public partial interface ITypedNotFound<TError>
    : INotFound, ITypedError<TError>
    where TError : ITypedNotFound<TError>;
