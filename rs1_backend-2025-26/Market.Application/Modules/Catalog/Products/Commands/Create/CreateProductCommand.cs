namespace Market.Application.Modules.Catalog.Products.Commands.Create;

public class CreateProductCommand : IRequest<int>
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
}