// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Results;

public partial record struct Success : ITypedSuccess<Success>;

public partial record struct Success<TValue>(TValue Value)
    : ITypedSuccess<Success<TValue>, TValue>;

public static partial class Result
{
    public static Success Success() => new();
    public static Success<TValue> Success<TValue>(this TValue value) => new(value);
}

public partial record struct Success<TValue, TResult1>
    : ITypedResults<Success<TValue, TResult1>, Success<TValue>, TResult1>
    where TResult1 : IResult;
public partial record struct Success<TValue, TResult1>
    : ITypedResults<Success<TValue, TResult1>, Success<TValue>, TResult1>
    where TResult1 : IResult;

public partial record struct Success<TValue, TResult1, TResult2>
    : ITypedResults<Success<TValue, TResult1, TResult2>, Success<TValue>, TResult1, TResult2>
    where TResult1 : IResult
    where TResult2 : IResult;

public partial record struct Success<TValue, TResult1, TResult2, TResult3>
    : ITypedResults<Success<TValue, TResult1, TResult2, TResult3>, Success<TValue>, TResult1, TResult2, TResult3>
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult;

public partial record struct Success<TValue, TResult1, TResult2, TResult3, TResult4>
    : ITypedResults<Success<TValue, TResult1, TResult2, TResult3, TResult4>, Success<TValue>, TResult1, TResult2, TResult3, TResult4>
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult;

public partial record struct Success<TValue, TResult1, TResult2, TResult3, TResult4, TResult5>
    : ITypedResults<Success<TValue, TResult1, TResult2, TResult3, TResult4, TResult5>, Success<TValue>, TResult1, TResult2, TResult3, TResult4, TResult5>
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
    where TResult5 : IResult;

public partial record struct Success<TValue, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>
    : ITypedResults<Success<TValue, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>, Success<TValue>, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
    where TResult5 : IResult
    where TResult6 : IResult;

public partial record struct Success<TValue, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>
    : ITypedResults<Success<TValue, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>, Success<TValue>, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
    where TResult5 : IResult
    where TResult6 : IResult
    where TResult7 : IResult;

public partial record struct Success<TValue, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>
    : ITypedResults<Success<TValue, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>, Success<TValue>, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
    where TResult5 : IResult
    where TResult6 : IResult
    where TResult7 : IResult
    where TResult8 : IResult;

public partial record struct Success<TValue, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>
    : ITypedResults<Success<TValue, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>, Success<TValue>, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
    where TResult5 : IResult
    where TResult6 : IResult
    where TResult7 : IResult
    where TResult8 : IResult
    where TResult9 : IResult;
