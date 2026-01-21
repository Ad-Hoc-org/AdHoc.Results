# AdHoc.Results

A modern, type-safe Result pattern implementation for .NET that eliminates exceptions as control flow and provides compile-time guarantees for error handling.

## Why the Result Pattern?

Traditional exception-based error handling in C# has several drawbacks:

- **Performance overhead**: Throwing and catching exceptions is expensive
- **Hidden control flow**: Methods don't declare what errors they can produce
- **Poor discoverability**: Developers must read documentation or source code to know what can go wrong
- **Difficult testing**: Exception paths are harder to test comprehensively

The Result pattern solves these problems by making errors **explicit, typed, and composable**.

## Why Typed Results?

While simple `IResult<TValue>` types are useful, **typed results** take error handling to the next level:

### Compile-Time Safety
Native language support with pattern matching ensures all possible outcomes are handled at compile time, eliminating runtime surprises.
```cs
// The compiler/analyzers KNOWS all possible outcomes
Results<Success<string>, InvalidData, NotFound, Unauthorized> content = ReadFile(path);
// You MUST handle all cases - no surprises at runtime
Results<Success<int>, InvalidData, Unprocessable, Unauthorized> containingNumber = content switch
{
    Success<string> { Value: var value } =>
        int.TryParse(value, out var result) ? result.Success() : path.Unprocessable(),
    NotFound notFound => Unprocessable(notFound.Message, notFound),
    var variant => new(variant)
};
```

### Self-Documenting APIs
```cs
// Method signature tells you EXACTLY what can happen
Results<Success<Config>, InvalidData, Unprocessable, Unauthorized> ReadConfig(string path) { /* Implementation */ }
```

### Built-in Diagnostics
Don't loose the conformance and debugging benefits of exceptions.
`Error`s can automatically capture stack trace information by using version tag trace.
For release builds, stack traces shouldn't be used to improve performance.

### ASP.NET Core Integration
Seamlessly integrate with ASP.NET Core with automatic HTTP status code mapping and problem details:
```cs
// Automatically maps to 200, 404, 401, 403
app.MapGet("/data", Results<Success<Data>, NotFound, Unauthorized, Forbidden> (int id) => /* Implementation */ );
```
Only with version tag `aspnet` available.

## Features

- ✅ **Pattern matching** support for elegant result/error handling
- ✅ **Roslyn analyzers** that catch incorrect usage at compile time
- ✅ **Type-safe error handling** with compile-time guarantees
- ✅ **Stack trace capture** for debugging (optional)
- ✅ **ASP.NET Core integration** with automatic HTTP status mapping
- ✅ **Problem Details** (RFC 7807) support for ASP.NET Core

## Custom Result Types

AdHoc.Results makes it easy to create your own custom result and error types using source generators.
Simply inherit from the appropriate interface and `AdHoc.Results.SourceGenerators` will automatically generate the necessary boilerplate code.

## Contributing

If you have suggestions for how this project could be improved, or want to report a bug, open an issue! We'd love all and any contributions.

For more, check out the [Contributing Guide](CONTRIBUTING.md).

## License

This project is licensed under the MIT License - see the LICENSE file for details.
