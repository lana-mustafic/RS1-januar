using Market.Application.Abstractions.Caching;

namespace Market.Application.Modules.Catalog.Products.Commands.Create;

public class CreateProductCommandHandler(
    IAppDbContext ctx,
    ICatalogCacheVersionService cacheVersionService) : IRequestHandler<CreateProductCommand, int>
{
    public async Task<int> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var normalized = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ValidationException("Name is required.");

        // Check if a product with the same name already exists.
        bool exists = await ctx.Products
            .AnyAsync(x => x.Name == normalized, ct);

        if (exists)
        {
            throw new MarketConflictException("Name already exists.");
        }

        var productCategory = await ctx.ProductCategories
            .Where(x => x.Id == request.CategoryId)
            .FirstOrDefaultAsync(ct);

        if (productCategory is null)
        {
            throw new MarketNotFoundException("ProductCategory", request.CategoryId);
        }

        if (productCategory.IsEnabled == false)
        {
            throw new MarketConflictException($"Category {productCategory.Name} is disabled.");
        }

        var product = new ProductEntity
        {
            Name = request.Name!.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            StockQuantity = 0, // default StockQuantity
            CategoryId = request.CategoryId,
            IsEnabled = true // deault IsEnabled
        };

        ctx.Products.Add(product);
        await ctx.SaveChangesAsync(ct);

        // Invalidate catalog cache
        await cacheVersionService.BumpVersionAsync(ct);

        return product.Id;
    }
}
