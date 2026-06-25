namespace Market.Application.Modules.Catalog.ProductCategories.Queries.GetById;

public class GetProductCategoryByIdQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required bool IsEnabled { get; init; }
}
