using Market.Domain.Entities.Sales;

namespace Market.Application.Modules.Sales.Orders.Commands.Status
{
    /// <summary>
    /// Command to change order status
    /// </summary>
    public class ChangeOrderStatusCommand : IRequest
    {
        public int Id { get; set; }
        public OrderStatusType NewStatus { get; set; }
    }
}
