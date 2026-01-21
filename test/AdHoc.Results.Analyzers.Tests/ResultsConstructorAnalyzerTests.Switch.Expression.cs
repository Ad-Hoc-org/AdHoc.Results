// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis.Testing;

namespace AdHoc.Results.Analyzers.Tests;

public partial class ResultsConstructorAnalyzerTests
{
    [Fact]
    public async Task Switch_Expresssion_ShouldNotReportDiagnostic()
    {
        await VerifiySwitchExpression(
            """
            Success => new(result.Variant),
            Success<int> => new(((Success<int>)result.Variant).Value.ToString().Success()),
            _ => new(result.Variant)
            """
        );
        await VerifiySwitchExpression(
            """
            Success<int> i => new(i.Value.ToString().Success()),
            _ => new(result.Variant)
            """
        );
        await VerifiySwitchExpression(
            """
            Success<int> { Value: var i } => new(i.ToString().Success()),
            {} variant => new(variant)
            """
        );
    }

    [Fact]
    public async Task Switch_Expresssion_ShouldReportDiagnostic()
    {
        var result = DiagnosticResult
            .CompilerError(ResultsConstructorAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("IResult", "Results<Success, Success<string>>");
        await VerifiySwitchExpression("_ => new({|#0:result.Variant|})", result);
        await VerifiySwitchExpression("{} variant => new({|#0:variant|})", result);
        await VerifiySwitchExpression(
            """
            Success<int> { Value: > 0 } i => new(i.Value.ToString().Success()),
            _ => new({|#0:result.Variant|})
            """,
            result
        );
    }

    private static Task VerifiySwitchExpression(
        string expressions,
        params DiagnosticResult[] expected
    ) => VerifyAnalyzerAsync($$"""
using AdHoc.Results.Abstractions;
using AdHoc.Results;

class Program
{
    static Results<Success, Success<string>> Main()
    {
        Results<Success, Success<int>, Success<string>> result = 42.Success();
        return result.Variant switch
        {
            {{expressions}}
        };
    }
}
""", expected);
}
