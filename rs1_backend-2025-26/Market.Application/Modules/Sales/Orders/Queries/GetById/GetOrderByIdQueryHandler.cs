namespace Market.Application.Modules.Sales.Orders.Queries.GetById;

public sealed class GetOrderByIdQueryHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
        : IRequestHandler<GetOrderByIdQuery, GetOrderByIdQueryDto>
{

    public async Task<GetOrderByIdQueryDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var q = ctx.Orders
            .Where(c => c.Id == request.Id);

        if (!currentUser.IsAdmin)
        {
            q = q.Where(x => x.MarketUserId == currentUser.UserId);
        }

        var dto = await q.OrderBy(x => x.OrderedAtUtc)
            .Select(x => new GetOrderByIdQueryDto
            {
                Id = x.Id,
                ReferenceNumber = x.ReferenceNumber,
                User = new GetByIdOrderQueryDtoUser
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
                Items = x.Items.Select(i => new GetByIdOrderQueryDtoItem
                {
                    Id = i.Id,
                    Product = new GetByIdOrderQueryDtoItemProduct
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
            }).FirstOrDefaultAsync(ct);

        if (dto == null)
        {
            throw new MarketNotFoundException($"Order with Id {request.Id} not found.");
        }

        return dto;
    }
}
