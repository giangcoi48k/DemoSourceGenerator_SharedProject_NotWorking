using BlazorApp.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherForecastService _weatherForecastService;

        public WeatherForecastController(IWeatherForecastService weatherForecastService)
        {
            _weatherForecastService = weatherForecastService;
        }

        [HttpPost]
        public async Task<IActionResult> GetForecasts()
        {
            return Ok(await _weatherForecastService.GetForecasts());
        }
    }
}
