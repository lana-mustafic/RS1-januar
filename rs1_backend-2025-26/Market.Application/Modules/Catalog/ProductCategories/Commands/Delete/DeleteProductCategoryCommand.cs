namespace Market.Application.Modules.Catalog.ProductCategories.Commands.Delete;

public class DeleteProductCategoryCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
