// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis.Testing;

namespace AdHoc.Results.Analyzers.Tests;

public partial class ResultsConstructorAnalyzerTests
{
    [Fact]
    public async Task If_ShouldNotReportDiagnostic()
    {
        await Verify(
            """
            if (result.Variant is Success<int> i)
                return i.Value.ToString().Success();
            else
                return new(result.Variant);
            """
        );
        await Verify(
            """
            if (result.Variant is Success<int> { Value: var i })
                return i.ToString().Success();
            return new(result.Variant);
            """
        );
    }

    [Fact]
    public async Task If_ShouldReportDiagnostic()
    {
        var result = DiagnosticResult
            .CompilerError(ResultsConstructorAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("IResult", "Results<Success, Success<string>>");
        await Verify(
            """
            if (result.Variant is Success<int> i)
                if (i.Value > 0)
                    return i.Value.ToString().Success();
            return new({|#0:result.Variant|});
            """,
            result
        );
        await Verify(
            """
            if (result.Variant is Success<int> { Value: > 0 } i)
                return i.Value.ToString().Success();
            return new({|#0:result.Variant|});
            """,
            result
        );
    }

    [Fact]
    public Task Conditional_ShouldNotReportDiagnostic() =>
        Verify(
            """
            return result.Variant is Success<int> i
                ? new(i.Value.ToString().Success())
                : new(result.Variant);
            """
        );

    [Fact]
    public async Task Conditional_ShouldReportDiagnostic()
    {
        var result = DiagnosticResult
            .CompilerError(ResultsConstructorAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("IResult", "Results<Success, Success<string>>");
        await Verify(
            """
            return result.Variant is Success<int> { Value: > 0 } i
                ? new(i.Value.ToString().Success())
                : new({|#0:result.Variant|});
            """,
            result
        );
    }
}
