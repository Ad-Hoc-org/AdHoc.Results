// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.OpenApi;

namespace AdHoc.Results.AspNetCore.OpenApi;

public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions AddAdHocResults(this OpenApiOptions options)
    {
        //options.AddSchemaTransformer<ErrorTypeOperationTransformer>();
        options.AddOperationTransformer<ResultOperationTransformer>();
        return options;
    }
}
