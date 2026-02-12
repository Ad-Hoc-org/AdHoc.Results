// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace AdHoc.Results;

public partial record Timeout : Error, ITypedTimeout<Timeout>;

public static partial class Result
{
    [StackTraceHidden]
    public static Timeout Timeout() => new();
}
