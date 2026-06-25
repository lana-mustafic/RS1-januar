namespace Market.Application.Modules.Catalog.Products.Commands.Delete;

public class DeleteProductCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
