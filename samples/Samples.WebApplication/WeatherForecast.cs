namespace Samples.WebApplication;

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    private static readonly string[] Summaries = [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public static Results<Success<IEnumerable<WeatherForecast>>, NoData, InvalidData> GenerateRandomForecast(int x) => x switch
    {
        0 => NoData(),
        > 0 => Enumerable.Range(1, x).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]
            )).Success(),
        _ => x.InvalidData()
    };

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
