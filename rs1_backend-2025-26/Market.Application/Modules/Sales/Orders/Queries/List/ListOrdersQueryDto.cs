using Market.Domain.Entities.Sales;

namespace Market.Application.Modules.Sales.Orders.Queries.List;

public sealed class ListOrdersQueryDto
{
    public required int Id { get; init; }
    public required string ReferenceNumber { get; init; }
    public required ListOrdersQueryDtoUser User { get; init; }
    public required DateTime OrderedAtUtc { get; set; }
    public required DateTime? PaidAtUtc { get; set; }
    public required OrderStatusType Status { get; set; }
    public required decimal TotalAmount { get; set; }
    public required string? Note { get; set; }

}
public sealed class ListOrdersQueryDtoUser
{
    public required string UserFirstname { get; set; }
    public required string UserLastname { get; set; }
    public required string UserAddress { get; set; }//todo
    public required string UserCity { get; set; }//todo
}
