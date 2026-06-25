using Market.Application.Abstractions.Caching;

namespace Market.Application.Modules.Catalog.ProductCategories.Commands.Status.Disable;

public sealed class DisableProductCategoryCommandHandler(
    IAppDbContext ctx,
    ICatalogCacheVersionService cacheVersionService) : IRequestHandler<DisableProductCategoryCommand, Unit>
{
    public async Task<Unit> Handle(DisableProductCategoryCommand request, CancellationToken ct)
    {
        var cat = await ctx.ProductCategories
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (cat is null)
        {
            throw new MarketNotFoundException($"Kategorija (ID={request.Id}) nije pronađena.");
        }

        if (!cat.IsEnabled) return Unit.Value; // idempotent

        // Business rule: cannot disable if there are active products
        var hasActiveProducts = await ctx.Products
            .AnyAsync(p => p.CategoryId == cat.Id && p.IsEnabled, ct);

        if (hasActiveProducts)
        {
            throw new MarketBusinessRuleException("category.disable.blocked.activeProducts",
                $"Category {cat.Name} cannot be disabled because it contains active products.");
        }

        cat.IsEnabled = false;

        await ctx.SaveChangesAsync(ct);

        // Invalidate catalog cache
        await cacheVersionService.BumpVersionAsync(ct);

        // await _bus.PublishAsync(new ProductCategoryDisabledV1IntegrationEvent(cat.Id, ...), ct);
        // await _cache.RemoveAsync(CacheKeys.CategoriesList, ct);

        return Unit.Value;
    }
}
