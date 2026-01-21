// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis.Testing;

namespace AdHoc.Results.Analyzers.Tests;

public partial class ResultsConstructorAnalyzerTests
{
    [Fact]
    public Task Variants_ShouldNotReportDiagnostic() =>
        VerifyAnalyzerAsync(@"
using AdHoc.Results.Abstractions;
using AdHoc.Results;

class Program
{
    static void Main()
    {
        Success<int> result = 42;
        Results<Success<int>, Success<string>> results = result;
        Results<Success, Success<string>, Success<int>> other = new(results.Variant);
    }
}");

    [Fact]
    public Task Variants_ShouldReportDiagnostic() =>
        VerifyAnalyzerAsync(@"
using AdHoc.Results.Abstractions;
using AdHoc.Results;

class Program
{
    static void Invalid()
    {
        Success<double> result = 3.14.Success();
        Results<Success<int>, Success<string>> other = new({|#0:result|});
    }
}", DiagnosticResult
            .CompilerError(ResultsConstructorAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("Success<double>", "Results<Success<int>, Success<string>>")
        );
}
