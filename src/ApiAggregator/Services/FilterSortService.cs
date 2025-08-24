using ApiAggregator.Models;

namespace ApiAggregator.Services;

public class FilterSortService
{
    public List<NewsArticle> FilterAndSortNews(IEnumerable<NewsArticle> news, AggregateQuery q)
    {
        var filtered = news.Where(a =>
        (q.From == null || a.PublishedAt >= q.From) &&
        (q.To == null || a.PublishedAt <= q.To) &&
        (string.IsNullOrWhiteSpace(q.Category) || string.Equals(a.Category, q.Category, StringComparison.OrdinalIgnoreCase))
        );

        return (q.Sort?.ToLowerInvariant()) switch
        {
            "date_asc" => filtered.OrderBy(a => a.PublishedAt).ToList(),
            "relevance" => filtered.ToList(), 
            _ => filtered.OrderByDescending(a => a.PublishedAt).ToList(),
        };
    }

    public List<RepoInfo> SortRepos(IEnumerable<RepoInfo> repos, string? sort, int take)
    {
        IEnumerable<RepoInfo> ordered = (sort?.ToLowerInvariant()) switch
        {
            "stars_asc" => repos.OrderBy(r => r.Stars),
            _ => repos.OrderByDescending(r => r.Stars)
        };
        return ordered.Take(Math.Max(1, take)).ToList();
    }
}