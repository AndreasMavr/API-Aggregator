using ApiAggregator.Models;

namespace ApiAggregator.External;

public interface INewsClient
{
    Task<Result<List<NewsArticle>>> SearchAsync(string query, DateTimeOffset? from, DateTimeOffset? to, int pageSize, CancellationToken ct);
}