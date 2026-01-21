// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface IConflict : IError;

public partial interface ITypedConflict<TError>
    : IConflict, ITypedError<TError>
    where TError : ITypedConflict<TError>;
