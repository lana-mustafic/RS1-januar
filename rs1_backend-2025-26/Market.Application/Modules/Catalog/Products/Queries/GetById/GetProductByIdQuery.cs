namespace Market.Application.Modules.Catalog.Products.Queries.GetById;

public class GetProductByIdQuery : IRequest<GetProductByIdQueryDto>
{
    public int Id { get; set; }
}