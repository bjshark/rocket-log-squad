namespace RocketLog.Api.Models.Common;

public sealed record PagedResponse<TItem>(
    IReadOnlyList<TItem> Items,
    long Total,
    int Page,
    int PageSize);
