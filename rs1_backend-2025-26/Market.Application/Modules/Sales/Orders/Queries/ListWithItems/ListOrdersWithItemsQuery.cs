namespace Market.Application.Modules.Sales.Orders.Queries.ListWithItems;

public sealed class ListOrdersWithItemsQuery : BasePagedQuery<ListOrdersWithItemsQueryDto>
{
    public string? Search { get; init; }
}
