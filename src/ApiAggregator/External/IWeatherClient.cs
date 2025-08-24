using ApiAggregator.Models;

namespace ApiAggregator.External;

public interface IWeatherClient
{
    Task<Result<WeatherSummary>> GetWeatherAsync(string city, CancellationToken ct);
}