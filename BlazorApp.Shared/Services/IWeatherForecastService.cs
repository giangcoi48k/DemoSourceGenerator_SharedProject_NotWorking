using BlazorApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.Shared.Services
{
    public interface IWeatherForecastService
    {
        Task<List<WeatherForecast>> GetForecasts();
    }
}
