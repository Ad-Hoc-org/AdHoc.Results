// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace AdHoc.Results;

public partial record NotSupported : Error, ITypedNotSupported<NotSupported>;

public static partial class Result
{
    [StackTraceHidden]
    public static NotSupported NotSupported() => new();
}
