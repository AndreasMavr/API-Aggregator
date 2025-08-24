using System.Diagnostics;

namespace ApiAggregator.Infrastructure;

public class TelemetryHandler : DelegatingHandler
{
    private readonly string _apiName;
    private readonly IStatsStore _store;
    private readonly ILogger<TelemetryHandler> _logger;

    public TelemetryHandler(string apiName, IStatsStore store, ILogger<TelemetryHandler> logger)
    { _apiName = apiName; _store = store; _logger = logger; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            sw.Stop();
            _store.Record(_apiName, sw.ElapsedMilliseconds);
        }
    }
}