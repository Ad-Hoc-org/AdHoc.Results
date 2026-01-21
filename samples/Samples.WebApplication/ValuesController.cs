#if !AOT
using Microsoft.AspNetCore.Mvc;

namespace Samples.WebApplication;

[Route("api/[controller]")]
[ApiController]
public class WeatherForecastController : ControllerBase
{
    [HttpGet]
    public Results<Success<IEnumerable<WeatherForecast>>, NoData, InvalidData> Get(int x = 5) =>
        WeatherForecast.GenerateRandomForecast(x);
}
#endif
