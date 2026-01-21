// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results.Abstractions;

public partial interface ITypedResults<TResult> : IResults
    where TResult : ITypedResults<TResult>;
