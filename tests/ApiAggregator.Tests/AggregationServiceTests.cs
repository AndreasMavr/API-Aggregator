using Xunit;
using ApiAggregator.External;
using ApiAggregator.Models;
using ApiAggregator.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;


public class AggregationServiceTests
{
    [Fact]
    public async Task Returns_Partial_With_Errors_When_One_Fails()
    {
        var weather = new Mock<IWeatherClient>();
        weather.Setup(w => w.GetWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<WeatherSummary>.Success(new WeatherSummary("Athens", "Clear", 30, 40, DateTimeOffset.UtcNow)));


        var news = new Mock<INewsClient>();
        news.Setup(n => n.SearchAsync(It.IsAny<string>(), null, null, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<List<NewsArticle>>.Failure("NewsApi", "boom"));


        var gh = new Mock<IGitHubClient>();
        gh.Setup(g => g.SearchReposAsync("dotnet", 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<List<RepoInfo>>.Success(new List<RepoInfo> { new("repo", "me", 1, "u", null) }));


        var svc = new AggregationService(weather.Object, news.Object, gh.Object, new FilterSortService(),
        Options.Create(new WeatherOptions()), Options.Create(new NewsOptions()));


        var res = await svc.GetAggregatedAsync(new AggregateQuery { PageSize = 10 }, CancellationToken.None);


        res.Weather.Should().NotBeNull();
        res.News.Should().BeEmpty();
        res.Repos.Should().HaveCount(1);
        res.Errors.Should().ContainSingle(e => e.Source == "NewsApi");
    }
}