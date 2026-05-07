namespace MicroLendingSystem.Shared.Models;

public sealed class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }

    public static Result<T> Success(T data) =>
        new() { IsSuccess = true, Data = data, StatusCode = 200 };

    public static Result<T> Failure(string error, int statusCode) =>
        new() { IsSuccess = false, Error = error, StatusCode = statusCode };
}

public sealed class PagedResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }

    public static PagedResult<T> Success(T data) =>
        new() { IsSuccess = true, Data = data, StatusCode = 200 };

    public static PagedResult<T> Failure(string error, int statusCode) =>
        new() { IsSuccess = false, Error = error, StatusCode = statusCode };
}

/// <summary>Payload for paginated endpoints used with <see cref="PagedResult{T}"/>.</summary>
public sealed class PagedPayload<TItem>
{
    public IReadOnlyList<TItem> Items { get; init; } = Array.Empty<TItem>();
    public int TotalCount { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
}
