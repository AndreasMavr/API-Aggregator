namespace ApiAggregator.Models;

public record ErrorInfo(string Source, string Message);

public record Result<T>
{
    public T? Data { get; init; }
    public ErrorInfo? Error { get; init; }
    public bool FromCache { get; init; }

    public static Result<T> Success(T data, bool fromCache = false) => new() { Data = data, FromCache = fromCache };
    public static Result<T> Failure(string source, string message) => new() { Error = new ErrorInfo(source, message) };
}