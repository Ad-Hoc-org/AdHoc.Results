// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace AdHoc.Results.Abstractions;

public interface IResultVariantsProvider
{
    static abstract ImmutableArray<Type> Variants { get; }
}
