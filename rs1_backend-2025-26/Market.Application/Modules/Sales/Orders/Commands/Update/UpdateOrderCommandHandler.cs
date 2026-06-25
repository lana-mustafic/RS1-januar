using Market.Domain.Entities.Sales;

namespace Market.Application.Modules.Sales.Orders.Commands.Update;

public class UpdateOrderCommandHandler(IAppDbContext ctx, IAppCurrentUser currentUser)
    : IRequestHandler<UpdateOrderCommand, int>
{
    public async Task<int> Handle(UpdateOrderCommand request, CancellationToken ct)
    {
        #region Querying existing order and set basic properties
        var q = ctx.Orders
            .Where(x => x.Id == request.Id);

        if (!currentUser.IsAdmin)
        {
            q = q.Where(x => x.MarketUserId == currentUser.UserId);
        }

        var order = await q
            .FirstOrDefaultAsync(ct);

        if (order is null)
            throw new MarketNotFoundException($"Orders (ID={request.Id}) nije pronađen.");

        if (order.Status != OrderStatusType.Draft)
            throw new ValidationException("Only draft orders can be updated.");

        order.Note = request.Note;
        order.TotalAmount = 0m;
        #endregion

        #region Query existing order items

        List<OrderItemEntity> exisingOrderItems = await ctx.OrderItems
                   .Where(oi => oi.OrderId == order.Id)
                   //.AsNoTracking() // ne smije biti AsNoTracking jer želimo update orderItema
                   .ToListAsync(ct); //<--- prihvatljivo za peformanse: jedan (1) sql upit za sve iteme

        Dictionary<int, OrderItemEntity> exisingOrderItemsMap = exisingOrderItems.ToDictionary(x => x.Id); //mapa postojecih itema po ID-u

        #endregion

        #region Delete order items items that are not in the API request
        //todo: brisanje starih itema

        var itemsToDelete = exisingOrderItems
            .Where(oi => request.Items.All(ri => ri.Id != oi.Id))
            .ToList();

        ctx.OrderItems.RemoveRange(itemsToDelete);

        #endregion

        #region Check if all items are deleted - auto cancel
        
        // Ako nema stavki u request-u, automatski prebaci na Cancelled status
        if (request.Items == null || request.Items.Count == 0)
        {
            order.Status = OrderStatusType.Cancelled;
            await ctx.SaveChangesAsync(ct);
            return order.Id;
        }
        
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

        #region Querying existing or create order items and calculate totals

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
            OrderItemEntity? orderItem = null;

            //jel insert
            if (item.Id == 0)
            {
                // insert logic
                orderItem = new OrderItemEntity
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
            }
            else
            {
                //update logic

                //orderItem = orderItems
                //    .Where(oi => oi.Id == item.Id) // ovo je O(n2)
                //    .FirstOrDefault(); //<--- nije kriticno za peformanse: ovo nije sql upit !!

                orderItem = exisingOrderItemsMap.GetValueOrDefault(item.Id); //<--- bolja performansa O(1) jer koristi dictionary

                if (orderItem is null)
                {
                    throw new ValidationException($"Order item (ID={item.Id}) not found in order (ID={order.Id}).");
                }

                //orderItem.OrderId = order.Id; // ne moze se menjati
                orderItem.ProductId = item.ProductId;//todo: dozvoliti menjanje proizvoda?
                orderItem.Quantity = item.Quantity;
                orderItem.UnitPrice = product.Price;
                orderItem.Subtotal = subtotal;
                orderItem.DiscountPercent = discountPercent;
                orderItem.DiscountAmount = discountAmount;
                orderItem.Total = total;
            }

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
