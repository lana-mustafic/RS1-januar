namespace Market.Application.Modules.Catalog.ProductCategories.Queries.GetById;

public class GetProductCategoryByIdQuery : IRequest<GetProductCategoryByIdQueryDto>
{
    public int Id { get; set; }
}