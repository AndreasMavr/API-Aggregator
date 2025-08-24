using ApiAggregator.Models;

namespace ApiAggregator.External;

public interface IGitHubClient
{
    Task<Result<List<RepoInfo>>> SearchReposAsync(string topic, int topN, CancellationToken ct);
}