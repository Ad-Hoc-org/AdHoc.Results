#if AOT
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

#endif

using AdHoc.Results.AspNetCore.OpenApi;
using Samples.WebApplication;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
#if AOT
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});
#else
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi(options => options.AddAdHocResults());
#endif

var app = builder.Build();

#if !AOT
app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();
#endif

// Configure the HTTP request pipeline.

app.MapGet("/weatherforecast", WeatherForecast.GenerateRandomForecast);
app.MapGet("/", () => NoData());

app.Run();

#if AOT
[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default
)]
[JsonSerializable(typeof(IEnumerable<WeatherForecast>))]
[JsonSerializable(typeof(IEnumerable<ProblemDetails>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
#endif
