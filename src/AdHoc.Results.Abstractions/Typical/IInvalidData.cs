// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface IInvalidData : IError;

public partial interface ITypedInvalidData<TError>
    : IInvalidData, ITypedError<TError>
    where TError : ITypedInvalidData<TError>;
