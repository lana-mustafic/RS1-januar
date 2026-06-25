using Market.Application.Modules.Sales.Orders.Commands.Status;
using Market.Domain.Entities.Sales;

namespace Market.Application.Modules.Sales.Orders.Commands.ChangeStatus;



/// <summary>
/// Handler for changing order status
/// </summary>
public class ChangeOrderStatusCommandHandler(IAppDbContext db) : IRequestHandler<ChangeOrderStatusCommand>
{
    public async Task Handle(ChangeOrderStatusCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct)
            ?? throw new MarketNotFoundException($"{nameof(OrderEntity)}, {request.Id}");

        // Validate status transition
        ValidateStatusTransition(order.Status, request.NewStatus);

        // Update status
        order.Status = request.NewStatus;

        // If marking as paid, set paid date
        if (request.NewStatus == OrderStatusType.Paid && !order.PaidAtUtc.HasValue)
        {
            order.PaidAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }

    private static void ValidateStatusTransition(OrderStatusType current, OrderStatusType next)
    {
        // Define valid transitions
        var validTransitions = new Dictionary<OrderStatusType, OrderStatusType[]>
        {
            { OrderStatusType.Draft, new[] { OrderStatusType.Confirmed, OrderStatusType.Cancelled } },
            { OrderStatusType.Confirmed, new[] { OrderStatusType.Paid, OrderStatusType.Cancelled } },
            { OrderStatusType.Paid, new[] { OrderStatusType.Completed, OrderStatusType.Cancelled } },
            { OrderStatusType.Completed, Array.Empty<OrderStatusType>() },
            { OrderStatusType.Cancelled, Array.Empty<OrderStatusType>() }
        };

        if (!validTransitions.ContainsKey(current))
        {
            throw new MarketBusinessRuleException("123", $"Unknown current status: {current}");
        }

        if (!validTransitions[current].Contains(next))
        {
            throw new MarketBusinessRuleException("323",
                $"Invalid status transition from {current} to {next}. " +
                $"Allowed transitions: {string.Join(", ", validTransitions[current])}");
        }
    }
}