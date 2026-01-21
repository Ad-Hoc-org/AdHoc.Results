// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using AdHoc.Results.Abstractions;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace AdHoc.Results.Analyzers.Tests;

public partial class ResultsConstructorAnalyzerTests
{
    private static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<ResultsConstructorAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100
        };
        test.TestState.AdditionalReferences.Add(typeof(IResult).Assembly);
        test.TestState.AdditionalReferences.Add(typeof(Success).Assembly);

        test.TestState.AdditionalReferences.Add(typeof(IEndpointMetadataProvider).Assembly);
        test.TestState.AdditionalReferences.Add(typeof(IConvertToActionResult).Assembly);

        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }

    private static Task Verify(
        string body,
        params DiagnosticResult[] expected
    ) => VerifyAnalyzerAsync($$"""
using System;
using AdHoc.Results.Abstractions;
using AdHoc.Results;

class Program
{
    static Results<Success, Success<string>> Main()
    {
        Results<Success, Success<int>, Success<string>> result = 42.Success();
        {{body}}
    }
}
""", expected);
}
