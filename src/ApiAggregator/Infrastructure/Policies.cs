using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace ApiAggregator.Infrastructure;

public static class Policies
{
    public static IAsyncPolicy<HttpResponseMessage> BuildStandardPolicy()
    {
        var retry = HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));

        var timeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
        var circuit = HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromSeconds(30));

        return Policy.WrapAsync(retry, circuit, timeout);
    }
}