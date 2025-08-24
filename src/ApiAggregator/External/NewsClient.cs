using ApiAggregator.Infrastructure;
using ApiAggregator.Models;
using ApiAggregator.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ApiAggregator.External;

public class NewsClient : INewsClient
{
    private readonly IHttpClientFactory _http;
    private readonly IMemoryCache _cache;
    private readonly NewsOptions _opts;

    public NewsClient(IHttpClientFactory http, IMemoryCache cache, IOptions<NewsOptions> opts)
    { _http = http; _cache = cache; _opts = opts.Value; }

    public async Task<Result<List<NewsArticle>>> SearchAsync(
    string query, DateTimeOffset? from, DateTimeOffset? to, int pageSize, CancellationToken ct)
    {
        var key = CacheKeys.News(query, from, to, pageSize);
        if (_cache.TryGetValue(key, out List<NewsArticle>? cached))
            return Result<List<NewsArticle>>.Success(cached!, fromCache: true);

        try
        {
            var client = _http.CreateClient("NewsApi");
            var url = $"everything?q={Uri.EscapeDataString(query)}&pageSize={pageSize}&sortBy=publishedAt&apiKey={_opts.ApiKey}";
            if (from != null) url += $"&from={from:O}";
            if (to != null) url += $"&to={to:O}";

            var http = await client.GetAsync(url, ct);
            if (!http.IsSuccessStatusCode)
            {
                var reason = $"{(int)http.StatusCode} {http.ReasonPhrase}";
                var body = await http.Content.ReadAsStringAsync(ct);
                var message = $"NewsApi error: {reason}. Body: {body}".Trim();

                if (_cache.TryGetValue(CacheKeys.NewsLastGood(query, from, to, pageSize), out List<NewsArticle>? lastGood))
                    return Result<List<NewsArticle>>.Success(lastGood!, fromCache: true) with
                    { Error = new ErrorInfo("NewsApi", message) };

                return Result<List<NewsArticle>>.Failure("NewsApi", message);
            }

            var resp = await http.Content.ReadFromJsonAsync<NewsResponse>(cancellationToken: ct);
            if (resp?.articles is null)
                return Result<List<NewsArticle>>.Failure("NewsApi", "Empty response");

            var list = resp.articles.Select(a => new NewsArticle(
                a.title ?? "(untitled)",
                a.source?.name ?? "unknown",
                DateTimeOffset.TryParse(a.publishedAt, out var p) ? p : DateTimeOffset.MinValue,
                a.category,
                a.url ?? string.Empty
            )).ToList();

            _cache.Set(key, list, TimeSpan.FromSeconds(_opts.CacheSeconds));
            _cache.Set(CacheKeys.NewsLastGood(query, from, to, pageSize), list, TimeSpan.FromMinutes(30));
            return Result<List<NewsArticle>>.Success(list);
        }
        catch (Exception ex)
        {
            if (_cache.TryGetValue(CacheKeys.NewsLastGood(query, from, to, pageSize), out List<NewsArticle>? lastGood))
                return Result<List<NewsArticle>>.Success(lastGood!, fromCache: true) with
                { Error = new ErrorInfo("NewsApi", $"{ex.Message}; served from last-known-good") };

            return Result<List<NewsArticle>>.Failure("NewsApi", ex.Message);
        }
    }

    private record NewsResponse(string? status, int? totalResults, List<Article>? articles);
    private record Article(Source? source, string? author, string? title, string? description, string? url, string? publishedAt, string? content)
    {
        public string? category { get; set; }
    }
    private record Source(string? id, string? name);

}