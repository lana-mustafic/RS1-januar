namespace Market.Application.Modules.Catalog.Products.Commands.Update;

public sealed class UpdateProductCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
}
