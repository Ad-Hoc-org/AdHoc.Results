// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis.Testing;

namespace AdHoc.Results.Analyzers.Tests;

public partial class ResultsConstructorAnalyzerTests
{
    [Fact]
    public async Task Switch_Statement_ShouldNotReportDiagnostic()
    {
        await VerifySwitchStatement(
            """
            case Success<int> i: return i.Value.ToString().Success();
            case Success<double> { Value: var d }: return d.ToString().Success();
            default: return new(result.Variant);
            """
        );
        await VerifySwitchStatement(
            """
            case Success<int>:
            case Success<double>:
                return ((IValueResult)result.Variant).Value.ToString().Success();
            case var variant: return new(variant);
            """
        );
    }

    [Fact]
    public async Task Switch_Statement_ShouldReportDiagnostic()
    {
        var result = DiagnosticResult
            .CompilerError(ResultsConstructorAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("IResult", "Results<Success, Success<string>>");
        await VerifySwitchStatement("case var variant: return new({|#0:variant|});", result);
        await VerifySwitchStatement("default: return new({|#0:result.Variant|});", result);
        await VerifySwitchStatement(
            """
            case Success<int> i: return i.Value.ToString().Success();
            default: return new({|#0:result.Variant|});
            """,
            result
        );
    }

    private static Task VerifySwitchStatement(
        string cases,
        params DiagnosticResult[] expected
    ) => VerifyAnalyzerAsync($$"""
using AdHoc.Results.Abstractions;
using AdHoc.Results;

class Program
{
    static Results<Success, Success<string>> Main()
    {
        Results<Success, Success<int>, Success<double>, Success<string>> result = 42.Success();
        switch (result.Variant)
        {
            {{cases}}
        }
    }
}
""", expected);
}
