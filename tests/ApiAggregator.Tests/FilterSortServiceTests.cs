using Xunit;
using ApiAggregator.Models;
using ApiAggregator.Services;
using FluentAssertions;


public class FilterSortServiceTests
{
    [Fact]
    public void Filters_By_Date_Range()
    {
        var now = DateTimeOffset.UtcNow;
        var items = new List<NewsArticle>{
new("A","S", now.AddDays(-1), "tech", "#"),
new("B","S", now, "tech", "#")
};
        var svc = new FilterSortService();
        var res = svc.FilterAndSortNews(items, new() { From = now.AddHours(-12), To = now.AddHours(1) });
        res.Should().ContainSingle(x => x.Title == "B");
    }
}