// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results;

public partial record struct Accepted : ITypedAccepted<Accepted>;

public static partial class Result
{
    public static Accepted Accepted() => new();
}
