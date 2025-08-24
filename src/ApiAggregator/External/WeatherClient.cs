using ApiAggregator.Infrastructure;
using ApiAggregator.Models;
using ApiAggregator.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ApiAggregator.External;

public class WeatherClient : IWeatherClient
{
    private readonly IHttpClientFactory _http;
    private readonly IMemoryCache _cache;
    private readonly WeatherOptions _opts;

    public WeatherClient(IHttpClientFactory http, IMemoryCache cache, IOptions<WeatherOptions> opts)
    { _http = http; _cache = cache; _opts = opts.Value; }

    public async Task<Result<WeatherSummary>> GetWeatherAsync(string city, CancellationToken ct)
    {
        var key = CacheKeys.Weather(city);
        if (_cache.TryGetValue(key, out WeatherSummary? cached))
            return Result<WeatherSummary>.Success(cached!, fromCache: true);

        var client = _http.CreateClient("OpenWeatherMap");
        var url = $"weather?q={Uri.EscapeDataString(city)}&units=metric&appid={_opts.ApiKey}";

        var http = await client.GetAsync(url, ct);
        if (!http.IsSuccessStatusCode)
        {
            var reason = $"{(int)http.StatusCode} {http.ReasonPhrase}";
            throw new Exception($"OpenWeatherMap error: {reason}");
        }

        var resp = await http.Content.ReadFromJsonAsync<OpenWeatherResponse>(cancellationToken: ct);
        if (resp is null) throw new Exception("OpenWeatherMap: empty response");

        var summary = new WeatherSummary(
            city,
            resp.weather?.FirstOrDefault()?.main ?? "N/A",
            resp.main?.temp ?? 0,
            resp.main?.humidity ?? 0,
            DateTimeOffset.UtcNow
        );

        _cache.Set(key, summary, TimeSpan.FromSeconds(_opts.CacheSeconds));
        _cache.Set(CacheKeys.WeatherLastGood(city), summary, TimeSpan.FromMinutes(30));
        return Result<WeatherSummary>.Success(summary);
    }

    private record OpenWeatherResponse(List<WeatherItem> weather, MainItem main);
    private record WeatherItem(string main, string description);
    private record MainItem(double temp, int humidity);
}