using BlazorApp.Client.ClientLogic;
using BlazorApp.Shared.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5043/")
});

builder.Services.AddScoped<IWeatherForecastService, HttpClientWeatherForecastService>();

await builder.Build().RunAsync();
