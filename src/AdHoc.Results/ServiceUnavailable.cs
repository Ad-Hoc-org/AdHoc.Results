// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace AdHoc.Results;

public partial record ServiceUnavailable : Error, ITypedServiceUnavailable<ServiceUnavailable>;

public static partial class Result
{
    [StackTraceHidden]
    public static ServiceUnavailable ServiceUnavailable() => new();
}
