namespace Market.Application.Modules.Sales.Orders.Queries.ListWithItems;

public sealed class ListOrdersWithItemsQueryHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
        : IRequestHandler<ListOrdersWithItemsQuery, PageResult<ListOrdersWithItemsQueryDto>>
{

    public async Task<PageResult<ListOrdersWithItemsQueryDto>> Handle(ListOrdersWithItemsQuery request, CancellationToken ct)
    {
        var q = ctx.Orders.AsNoTracking();

        if (!currentUser.IsAdmin)
        {
            q = q.Where(x => x.MarketUserId == currentUser.UserId);
        }
        var searchTerm = request.Search?.Trim().ToLower() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            q = q.Where(x => x.ReferenceNumber.ToLower().Contains(searchTerm));
        }

        var projectedQuery = q.OrderBy(x => x.OrderedAtUtc)
            .Select(x => new ListOrdersWithItemsQueryDto
            {
                Id = x.Id,
                ReferenceNumber = x.ReferenceNumber,
                User = new ListOrdersWithItemsQueryDtoUser
                {
                    UserFirstname = x.MarketUser!.Firstname,
                    UserLastname = x.MarketUser!.Lastname,
                    UserAddress = "Todo",//todo: ticket no 126
                    UserCity = "Todo",//todo: ticket no 126
                },
                OrderedAtUtc = x.OrderedAtUtc,
                PaidAtUtc = x.PaidAtUtc,
                Status = x.Status,
                TotalAmount = x.TotalAmount,
                Note = x.Note,
                //"x.Items" ili "ctx.OrderItems.Where(x => x.OrderId == x.Id)"
                Items = x.Items.Select(i => new ListOrdersWithItemsQueryDtoItem
                {
                    Id = i.Id,
                    Product = new ListOrdersWithItemsQueryDtoItemProduct
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product!.Name,
                        ProductCategoryName = i.Product!.Category!.Name
                    },
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal,
                    DiscountAmount = i.DiscountAmount,
                    DiscountPercent = i.DiscountPercent,
                    Total = i.Total
                }).ToList()
            });

        return await PageResult<ListOrdersWithItemsQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
