namespace Market.Application.Modules.Sales.Orders.Commands.Update;

public class UpdateOrderCommand : IRequest<int>
{
    [JsonIgnore]
    public int Id { get; set; }
    public string? Note { get; set; }

    public List<UpdateOrderCommandItem> Items { get; set; } = [];
}

public class UpdateOrderCommandItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
}