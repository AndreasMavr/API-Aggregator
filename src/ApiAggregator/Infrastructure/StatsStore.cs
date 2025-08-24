using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using ApiAggregator.Services;
namespace ApiAggregator.Infrastructure;

public interface IStatsStore
{
    void Record(string api, long elapsedMs);
    object ToSnapshot();
    double GetLast5MinAverage(string api);
    double GetOverallAverage(string api);
}

internal class StatsBuckets
{
    public long TotalRequests;
    public long TotalElapsedMs;
    public long Fast;
    public long Medium;
    public long Slow;
    public ConcurrentQueue<(DateTimeOffset ts, long ms)> Recent = new();
}

public class StatsStore : IStatsStore
{
    private readonly ConcurrentDictionary<string, StatsBuckets> _byApi = new();
    private readonly StatsOptions _opts;

    public StatsStore(IOptions<StatsOptions> opts) => _opts = opts.Value;

    public void Record(string api, long elapsedMs)
    {
        var s = _byApi.GetOrAdd(api, _ => new StatsBuckets());
        Interlocked.Increment(ref s.TotalRequests);
        Interlocked.Add(ref s.TotalElapsedMs, elapsedMs);

        if (elapsedMs < _opts.FastMs) Interlocked.Increment(ref s.Fast);
        else if (elapsedMs < _opts.MediumMs) Interlocked.Increment(ref s.Medium);
        else Interlocked.Increment(ref s.Slow);

        var now = DateTimeOffset.UtcNow;
        s.Recent.Enqueue((now, elapsedMs));
        while (s.Recent.TryPeek(out var head) && now - head.ts > TimeSpan.FromMinutes(5))
            s.Recent.TryDequeue(out _);
    }

    public object ToSnapshot()
    {
        var dict = new Dictionary<string, object>();
        foreach (var kvp in _byApi)
        {
            var s = kvp.Value;
            var total = Math.Max(1, Interlocked.Read(ref s.TotalRequests));
            var avg = (double)Interlocked.Read(ref s.TotalElapsedMs) / total;
            dict[kvp.Key] = new
            {
                totalRequests = total,
                averageMs = Math.Round(avg, 2),
                buckets = new
                {
                    fast = Interlocked.Read(ref s.Fast),
                    medium = Interlocked.Read(ref s.Medium),
                    slow = Interlocked.Read(ref s.Slow)
                },
                last5MinAverageMs = Math.Round(GetLast5MinAverage(kvp.Key), 2)
            };
        }
        return dict;
    }

    public double GetLast5MinAverage(string api)
    {
        if (!_byApi.TryGetValue(api, out var s)) return 0;
        var now = DateTimeOffset.UtcNow;
        var list = s.Recent.Where(x => now - x.ts <= TimeSpan.FromMinutes(5)).ToArray();
        if (list.Length == 0) return 0;
        return list.Average(x => x.ms);
    }

    public double GetOverallAverage(string api)
    {
        if (!_byApi.TryGetValue(api, out var s)) return 0;
        var total = Math.Max(1, Interlocked.Read(ref s.TotalRequests));
        return (double)Interlocked.Read(ref s.TotalElapsedMs) / total;
    }
}
