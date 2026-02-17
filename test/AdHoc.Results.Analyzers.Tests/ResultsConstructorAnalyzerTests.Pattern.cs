// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis.Testing;

namespace AdHoc.Results.Analyzers.Tests;

public partial class ResultsConstructorAnalyzerTests
{
    [Fact]
    public async Task Pattern_Type_ShouldNotReportDiagnostic()
    {
        await Verify(
            """
            if (result.Variant is Success s)
                return new(result.Variant);
            throw new Exception();
            """
        );
        await Verify(
            """
            if (result.Variant is Success<string> { Value: "" })
                return new(result.Variant);
            throw new Exception();
            """
        );
        await Verify(
            """
            if (result.Variant is Success or Success<string>)
                return new(result.Variant);
            throw new Exception();
            """
        );
        await Verify(
            """
            if (result.Variant is not Success<int>)
                return new(result.Variant);
            throw new Exception();
            """
        );
        await Verify(
            """
            if (result.Variant is not Success<int> intSuccess)
                return new(result.Variant);
            throw new Exception();
            """
        );
    }

    [Fact]
    public async Task Pattern_Type_Variants_ShouldNotReportDiagnostic()
    {
        await Verify(
            """
            if (result is { Variant: Success s })
                return new(result.Variant);
            throw new Exception();
            """
        );
        await Verify(
            """
            if (result is { Variant: Success<string> { Value: "" } })
                return new(result.Variant);
            throw new Exception();
            """
        );
        await Verify(
            """
            if (result is { Variant: Success or Success<string> })
                return new(result.Variant);
            throw new Exception();
            """
        );
        await Verify(
            """
            Results<Success<int>, Results<Success<string>, Success>> nested = 42.Success();
            if (nested is { Variant: Results<Success<string>, Success> variant })
                return new(variant.Variant);
            throw new Exception();
            """
        );
        await Verify(
            """
            Results<Success<int>, Results<Success<string>, Success>> nested = 42.Success();
            if (nested is { Variant: Results<Success<string>, Success> { Variant: var variant } })
                return new(variant);
            throw new Exception();
            """
        );
    }

    [Fact]
    public async Task Pattern_Type_ShouldReportDiagnostic()
    {
        var result = DiagnosticResult
            .CompilerError(ResultsConstructorAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("IResult", "Results<Success, Success<string>>");
        await Verify(
            """
            if (result.Variant is Success<int> { Value: > 0 } i)
                return new(i.Value.ToString().Success());
            else
                return new({|#0:result.Variant|});
            """,
            result
        );
        await Verify(
            """
            if (result.Variant is not Success<int> { Value: > 0 })
                return new({|#0:result.Variant|});
            throw new Exception();
            """,
            result
        );
    }

    [Fact]
    public async Task Pattern_Type_Variants_ShouldReportDiagnostic()
    {
        var result = DiagnosticResult
            .CompilerError(ResultsConstructorAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("IResult", "Results<Success, Success<string>>");
        await Verify(
            """
            if (result is { Variant: Success<int> { Value: > 0 } i })
                return new(i.Value.ToString().Success());
            else
                return new({|#0:result.Variant|});
            """,
            result
        );
        await Verify(
            """
            if (result is { Variant: not Success<int> { Value: > 0 } })
                return new({|#0:result.Variant|});
            throw new Exception();
            """,
            result
        ); await Verify(
            """
            if (result is { Variant: not Success<int> })
                return new({|#0:result.Variant|});
            throw new Exception();
            """,
            result
        );
    }
}
