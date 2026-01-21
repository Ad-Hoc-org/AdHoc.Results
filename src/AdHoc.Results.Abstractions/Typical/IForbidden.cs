// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface IForbidden : IError;

public partial interface ITypedForbidden<TError>
    : IForbidden, ITypedError<TError>
    where TError : ITypedForbidden<TError>;
