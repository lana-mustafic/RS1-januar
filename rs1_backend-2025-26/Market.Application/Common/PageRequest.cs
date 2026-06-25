namespace Market.Application.Common;

public sealed class PageRequest
{
    private const int MaxPageSize = 10000;
    public int Page { get; init; } = 1;

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value <= 0 ? 10 : value > MaxPageSize ? MaxPageSize : value;
    }
    [JsonIgnore]
    public int SkipCount => (Page - 1) * PageSize;
}
