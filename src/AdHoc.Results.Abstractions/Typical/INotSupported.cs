// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface INotSupported : IError;

public partial interface ITypedNotSupported<TError>
    : INotSupported, ITypedError<TError>
    where TError : ITypedNotSupported<TError>;
