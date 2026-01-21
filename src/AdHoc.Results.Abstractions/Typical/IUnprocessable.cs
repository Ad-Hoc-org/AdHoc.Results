// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface IUnprocessable : IError;

public partial interface ITypedUnprocessable<TError>
    : IUnprocessable, ITypedError<TError>
    where TError : ITypedUnprocessable<TError>;
