// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace AdHoc.Results;

public partial record struct NoData : ITypedNoData<NoData>;

public static partial class Result
{
    public static NoData NoData() => new();

    public static Results<Success<TValue>, NoData> Data<TValue>([AllowNull] this TValue value) =>
        value is null ? NoData() : Success(value);

    public static Results<Success<IEnumerable<TElement>>, NoData> DataOrEmpty<TElement>([AllowNull] this IEnumerable<TElement> value)
    {
        if (value is null)
            return NoData();
        if (value is ICollection collection)
            return collection.Count == 0 ? NoData() : Success(value);
        if (value is Array array)
            return array.Length == 0 ? NoData() : Success(value);

        value = [.. value];
        return value.Any() ? Success(value) : NoData();
    }
}
