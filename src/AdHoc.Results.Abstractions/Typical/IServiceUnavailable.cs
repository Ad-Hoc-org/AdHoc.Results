// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface IServiceUnavailable : IError;

public partial interface ITypedServiceUnavailable<TError>
    : IServiceUnavailable, ITypedError<TError>
    where TError : ITypedServiceUnavailable<TError>;
