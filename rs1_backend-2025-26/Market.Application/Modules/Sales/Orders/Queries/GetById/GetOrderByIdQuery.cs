namespace Market.Application.Modules.Sales.Orders.Queries.GetById;

public sealed class GetOrderByIdQuery : IRequest<GetOrderByIdQueryDto>
{
    public int Id { get; set; }
}
