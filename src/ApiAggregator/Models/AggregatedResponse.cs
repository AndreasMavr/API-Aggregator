namespace ApiAggregator.Models;

public record WeatherSummary(string City, string Summary, double TemperatureC, int Humidity, DateTimeOffset RetrievedAt);
public record NewsArticle(string Title, string Source, DateTimeOffset PublishedAt, string? Category, string Url);
public record RepoInfo(string Name, string Owner, int Stars, string HtmlUrl, string? Description);

public record AggregatedResponse(
WeatherSummary? Weather,
List<NewsArticle> News,
List<RepoInfo> Repos,
List<ErrorInfo> Errors
);