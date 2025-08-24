using ApiAggregator.Models;
using ApiAggregator.External;
using Microsoft.Extensions.Options;

namespace ApiAggregator.Services;

public class AggregationService
{
    private readonly IWeatherClient _weather;
    private readonly INewsClient _news;
    private readonly IGitHubClient _gitHub;
    private readonly FilterSortService _filterSort;
    private readonly WeatherOptions _wOpts;
    private readonly NewsOptions _nOpts;

    public AggregationService(IWeatherClient weather, INewsClient news, IGitHubClient gitHub,
    FilterSortService filterSort, IOptions<WeatherOptions> w, IOptions<NewsOptions> n)
    {
        _weather = weather; _news = news; _gitHub = gitHub; _filterSort = filterSort;
        _wOpts = w.Value; _nOpts = n.Value;
    }

    public async Task<AggregatedResponse> GetAggregatedAsync(AggregateQuery q, CancellationToken ct)
    {
        var city = string.IsNullOrWhiteSpace(q.City) ? _wOpts.DefaultCity : q.City!;
        var newsQuery = string.IsNullOrWhiteSpace(q.NewsQuery) ? _nOpts.DefaultQuery : q.NewsQuery!;
        var topic = string.IsNullOrWhiteSpace(q.ReposTopic) ? "dotnet" : q.ReposTopic!;
        var take = q.PageSize <= 0 ? 10 : q.PageSize;

        async Task<Result<T>> Safe<T>(Func<Task<Result<T>>> f, string src)
        {
            try { return await f(); }
            catch (Exception ex) { return Result<T>.Failure(src, ex.Message); }
        }

        var weatherTask = Safe(() => _weather.GetWeatherAsync(city, ct), "OpenWeatherMap");
        var newsTask = Safe(() => _news.SearchAsync(newsQuery, q.From, q.To, take, ct), "NewsApi");
        var reposTask = Safe(() => _gitHub.SearchReposAsync(topic, take, ct), "GitHub");

        await Task.WhenAll(weatherTask, newsTask, reposTask);

        var errors = new List<ErrorInfo>();

        var weather = weatherTask.Result.Data;
        if (weatherTask.Result.Error is not null) errors.Add(weatherTask.Result.Error);

        var news = newsTask.Result.Data ?? new List<NewsArticle>();
        if (newsTask.Result.Error is not null) errors.Add(newsTask.Result.Error);

        var repos = reposTask.Result.Data ?? new List<RepoInfo>();
        if (reposTask.Result.Error is not null) errors.Add(reposTask.Result.Error);

        news = _filterSort.FilterAndSortNews(news, q);
        repos = _filterSort.SortRepos(repos, q.Sort, take);

        return new AggregatedResponse(weather, news, repos, errors);
    }
}