using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public DateOnly Date { get; private set; }
    public int TemperatureC { get; private set; }
    public string Summary { get; private set; }

    [HttpGet]
    public IEnumerable<WeatherForecastController> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecastController
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
