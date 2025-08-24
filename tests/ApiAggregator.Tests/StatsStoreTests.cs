using Xunit;
using ApiAggregator.Infrastructure;
using ApiAggregator.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Text.Json;

public class StatsStoreTests
{
    private sealed class ApiStatsSnapshotDto
    {
        public long totalRequests { get; set; }
        public double averageMs { get; set; }
        public BucketsDto buckets { get; set; } = new();
        public double last5MinAverageMs { get; set; }
    }

    private sealed class BucketsDto
    {
        public long fast { get; set; }
        public long medium { get; set; }
        public long slow { get; set; }
    }

    [Fact]
    public void Buckets_And_Average_Work()
    {
        var store = new StatsStore(Options.Create(new StatsOptions { FastMs = 100, MediumMs = 200 }));
        store.Record("X", 30);
        store.Record("X", 70);
        store.Record("X", 170);

        var raw = (Dictionary<string, object>)store.ToSnapshot();

        var json = JsonSerializer.Serialize(raw);
        var typed = JsonSerializer.Deserialize<Dictionary<string, ApiStatsSnapshotDto>>(json)!;

        typed.Should().ContainKey("X");
        var x = typed["X"];

        x.totalRequests.Should().Be(3);
        x.buckets.fast.Should().Be(2);
        x.buckets.medium.Should().Be(1);
        x.buckets.slow.Should().Be(0);

        var expectedAvg = (30 + 70 + 170) / 3.0;
        x.averageMs.Should().BeApproximately(expectedAvg, 0.01);
    }
}
