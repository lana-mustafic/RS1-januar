using Market.Domain.Entities.Sales;

namespace Market.Application.Modules.Sales.Orders.Commands.Create;

public class CreateOrderCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<CreateOrderCommand, int>
{
    public async Task<int> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        #region Create order and set basic properties
        var order = new OrderEntity
        {
            ReferenceNumber = Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
            MarketUserId = currentUser.UserId!.Value,
            OrderedAtUtc = DateTime.UtcNow,
            Status = OrderStatusType.Draft,
            TotalAmount = 0m, //
            Note = request.Note
        };
        ctx.Orders.Add(order);
        #endregion

        #region Load products from database and prepare a map

        // pokupiti sve id-ove proizvoda koji se naručuju
        List<int> productIds = request.Items.Select(ri => ri.ProductId).ToList(); // ne treba hashset jer filter se radi u bazi

        List<ProductEntity> products = await ctx.Products
            .Where(p => productIds.Contains(p.Id)) //<-- dorada nakon nastave za poboljsanje performansi: filtrirati samo proizvode koji su u request.Items
            .AsNoTracking()
            .ToListAsync(ct);

        Dictionary<int, ProductEntity> productsMap = products.ToDictionary(x => x.Id);
        #endregion

        #region Create order items and calculate totals

        // za demo svrhe, svi proizvodi imaju 5% popusta
        decimal discountPercent = 0.05m;

        foreach (var item in request.Items)
        {
            ProductEntity? product = productsMap.GetValueOrDefault(item.ProductId); //<--- bolja performansa O(n) jer koristi dictionary

            if (product is null)
            {
                throw new ValidationException(message: $"Invalid productId {item.ProductId}.");
            }

            if (product.IsEnabled == false)
            {
                throw new ValidationException($"Product {product.Name} is disabled.");
            }

            decimal subtotal = RoundMoney(product.Price * item.Quantity);
            decimal discountAmount = RoundMoney(subtotal * discountPercent);
            decimal total = RoundMoney(subtotal - discountAmount);

            var orderItem = new OrderItemEntity
            {
                Order = order,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Subtotal = subtotal,
                DiscountPercent = discountPercent,
                DiscountAmount = discountAmount,
                Total = total
            };

            ctx.OrderItems.Add(orderItem);
            order.TotalAmount += RoundMoney(orderItem.Total);
        }
        #endregion

        await ctx.SaveChangesAsync(ct);

        return order.Id;
    }

    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}