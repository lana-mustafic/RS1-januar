using Market.Application.Abstractions.Caching;

namespace Market.Application.Modules.Catalog.Products.Commands.Update;

public sealed class UpdateProductCommandHandler(
    IAppDbContext ctx,
    ICatalogCacheVersionService cacheVersionService) : IRequestHandler<UpdateProductCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var entity = await ctx.Products
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            throw new MarketNotFoundException($"Product (ID={request.Id}) nije pronađena.");

        // Check for duplicate name (case-insensitive, except for the same ID)
        var exists = await ctx.Products
            .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == request.Name.ToLower(), ct);

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

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.Price = request.Price;
        entity.CategoryId = request.CategoryId;

        await ctx.SaveChangesAsync(ct);

        // Invalidate catalog cache
        await cacheVersionService.BumpVersionAsync(ct);

        return Unit.Value;
    }
}
