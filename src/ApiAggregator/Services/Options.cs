namespace ApiAggregator.Services;

public class WeatherOptions
{
    public string BaseUrl { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public string DefaultCity { get; set; } = "Athens";
    public int CacheSeconds { get; set; } = 60;
}

public class NewsOptions
{
    public string BaseUrl { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public string DefaultQuery { get; set; } = "technology";
    public int CacheSeconds { get; set; } = 90;
}

public class GitHubOptions
{
    public string BaseUrl { get; set; } = default!;
    public string? Token { get; set; }
    public int CacheSeconds { get; set; } = 120;
}

public class StatsOptions
{
    public int FastMs { get; set; } = 100;
    public int MediumMs { get; set; } = 200;
    public double AnomalyFactor { get; set; } = 1.5;
}