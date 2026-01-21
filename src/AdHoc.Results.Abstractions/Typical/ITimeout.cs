// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface ITimeout : IError;

public partial interface ITypedTimeout<TError>
    : ITimeout, ITypedError<TError>
    where TError : ITypedTimeout<TError>;
