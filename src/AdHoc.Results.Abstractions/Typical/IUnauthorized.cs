// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface IUnauthorized : IError;

public partial interface ITypedUnauthorized<TError>
    : IUnauthorized, ITypedError<TError>
    where TError : ITypedUnauthorized<TError>;
