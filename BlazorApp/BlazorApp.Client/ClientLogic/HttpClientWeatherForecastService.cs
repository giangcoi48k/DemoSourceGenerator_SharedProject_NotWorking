using BlazorApp.Shared.Models;
using BlazorApp.Shared.Services;
using System.Net.Http.Json;

namespace BlazorApp.Client.ClientLogic
{
    public class HttpClientWeatherForecastService(HttpClient httpClient) : IWeatherForecastService
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<List<WeatherForecast>> GetForecasts()
        {
            using var response = await _httpClient.PostAsync("api/WeatherForecast/GetForecasts", null);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>();
            return content!;
        }
    }
}
