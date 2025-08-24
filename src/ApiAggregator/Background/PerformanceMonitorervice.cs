using ApiAggregator.Infrastructure;
using ApiAggregator.Services;
using Microsoft.Extensions.Options;

namespace ApiAggregator.Background;

public class PerformanceMonitorService : BackgroundService
{
    private readonly ILogger<PerformanceMonitorService> _logger;
    private readonly IStatsStore _stats;
    private readonly StatsOptions _opts;

    public PerformanceMonitorService(ILogger<PerformanceMonitorService> logger, IStatsStore stats, IOptions<StatsOptions> opts)
    { _logger = logger; _stats = stats; _opts = opts.Value; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            var snap = (Dictionary<string, object>)_stats.ToSnapshot();
            foreach (var api in snap.Keys)
            {
                var last5 = _stats.GetLast5MinAverage(api);
                var overall = _stats.GetOverallAverage(api);
                if (overall > 0 && last5 > overall * _opts.AnomalyFactor)
                {
                    _logger.LogWarning("Performance anomaly for {Api}: last5={Last5}ms overall={Overall}ms", api, Math.Round(last5, 2), Math.Round(overall, 2));
                }
            }
        }
    }
}