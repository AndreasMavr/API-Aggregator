namespace ApiAggregator.Infrastructure;

public static class CacheKeys
{
    public static string Weather(string city) => $"weather:{city.ToLowerInvariant()}";
    public static string WeatherLastGood(string city) => $"weather:lastgood:{city.ToLowerInvariant()}";

    public static string News(string q, DateTimeOffset? from, DateTimeOffset? to, int page) =>
    $"news:{q.ToLowerInvariant()}:{from?.UtcDateTime:o}:{to?.UtcDateTime:o}:{page}";
    public static string NewsLastGood(string q, DateTimeOffset? from, DateTimeOffset? to, int page) =>
    $"news:lastgood:{q.ToLowerInvariant()}:{from?.UtcDateTime:o}:{to?.UtcDateTime:o}:{page}";

    public static string Repos(string topic, int topN) => $"repos:{topic.ToLowerInvariant()}:{topN}";
    public static string ReposLastGood(string topic, int topN) => $"repos:lastgood:{topic.ToLowerInvariant()}:{topN}";
}