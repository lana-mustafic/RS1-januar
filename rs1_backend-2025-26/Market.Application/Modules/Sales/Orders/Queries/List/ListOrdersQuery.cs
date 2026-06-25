namespace Market.Application.Modules.Sales.Orders.Queries.List;

public sealed class ListOrdersQuery : BasePagedQuery<ListOrdersQueryDto>
{
    public string? Search { get; init; }
}
