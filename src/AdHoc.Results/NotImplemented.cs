// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace AdHoc.Results;

public partial record NotImplemented : Error, ITypedNotSupported<NotImplemented>;

public static partial class Result
{
    [StackTraceHidden]
    public static NotImplemented NotImplemented() => new();
}
