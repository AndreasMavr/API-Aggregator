using ApiAggregator.Infrastructure;
using ApiAggregator.Models;
using ApiAggregator.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ApiAggregator.External;

public class GitHubClient : IGitHubClient
{
    private readonly IHttpClientFactory _http;
    private readonly IMemoryCache _cache;
    private readonly GitHubOptions _opts;

    public GitHubClient(IHttpClientFactory http, IMemoryCache cache, IOptions<GitHubOptions> opts)
    { _http = http; _cache = cache; _opts = opts.Value; }

    public async Task<Result<List<RepoInfo>>> SearchReposAsync(string topic, int topN, CancellationToken ct)
    {
        var key = CacheKeys.Repos(topic, topN);
        if (_cache.TryGetValue(key, out List<RepoInfo>? cached))
            return Result<List<RepoInfo>>.Success(cached!, fromCache: true);

        var client = _http.CreateClient("GitHub");
        var url = $"search/repositories?q=topic:{Uri.EscapeDataString(topic)}&sort=stars&order=desc&per_page={topN}";

        var http = await client.GetAsync(url, ct);
        if (!http.IsSuccessStatusCode)
        {
            var reason = $"{(int)http.StatusCode} {http.ReasonPhrase}";
            throw new Exception($"GitHub error: {reason}");
        }

        var resp = await http.Content.ReadFromJsonAsync<GitHubSearchResponse>(cancellationToken: ct);
        if (resp?.items is null) throw new Exception("GitHub: empty response");

        var list = resp.items.Select(i => new RepoInfo(
            i.name ?? "(unknown)",
            i.owner?.login ?? "(unknown)",
            i.stargazers_count,
            i.html_url ?? string.Empty,
            i.description
        )).ToList();

        _cache.Set(key, list, TimeSpan.FromSeconds(_opts.CacheSeconds));
        _cache.Set(CacheKeys.ReposLastGood(topic, topN), list, TimeSpan.FromMinutes(30));
        return Result<List<RepoInfo>>.Success(list);
    }

    private record GitHubSearchResponse(int total_count, bool incomplete_results, List<Item> items);
    private record Item(string name, string html_url, int stargazers_count, string? description, Owner owner);
    private record Owner(string login);
}