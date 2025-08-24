using System.ComponentModel.DataAnnotations;

namespace ApiAggregator.Models;

public class AggregateQuery
{
    public string? City { get; set; }
    public string? NewsQuery { get; set; }
    public string? Category { get; set; }
    public string? ReposTopic { get; set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset? From { get; set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset? To { get; set; }

    public int PageSize { get; set; } = 10;
    public string? Sort { get; set; }
}