using Market.Application.Abstractions;
using Market.Application.Abstractions.Caching;

namespace Market.Application.Modules.Catalog.CatalogHome.Queries.GetCatalogHome;

public sealed class GetCatalogHomeQueryHandler(
    IAppDbContext db,
    ICacheService cacheService,
    ICatalogCacheVersionService versionService) : IRequestHandler<GetCatalogHomeQuery, CatalogHomeDto>
{
    public async Task<CatalogHomeDto> Handle(GetCatalogHomeQuery request, CancellationToken cancellationToken)
    {
        // Clamp limits
        var categoryLimit = Math.Clamp(request.CategoryLimit, 1, 50);
        var productLimit = Math.Clamp(request.ProductLimit, 1, 100);

        // Get version
        var version = await versionService.GetVersionAsync(cancellationToken);

        // Build cache key with version and actual limits
        var cacheKey = $"catalog:v{version}:home:c{categoryLimit}:p{productLimit}";

        // Get or create cached result
        var result = await cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var now = DateTime.UtcNow;

                // Categories
                var categories = await db.ProductCategories
                    .AsNoTracking()
                    .Where(c => c.IsEnabled && !c.IsDeleted)
                    .OrderBy(c => c.Name)
                    .Take(categoryLimit)
                    .Select(c => new CatalogCategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToListAsync(ct);

                // Newest products (by CreatedAtUtc DESC, then Id DESC)
                // FIX: Changed from OrderByDescending(p => p.Id) to OrderByDescending(p => p.CreatedAtUtc)
                var products = await db.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.IsEnabled && !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAtUtc)  // ✅ FIXED: Sort by creation date
                    .ThenByDescending(p => p.Id)              // ✅ ADDED: Tiebreaker for same timestamp
                    .Take(productLimit)
                    .Select(p => new CatalogProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name
                    })
                    .ToListAsync(ct);

                // Promotions (active and within date window)
                var promotions = await db.Promotions
                    .AsNoTracking()
                    .Where(p => p.IsActive
                        && !p.IsDeleted
                        && (p.StartsAtUtc == null || p.StartsAtUtc <= now)
                        && (p.EndsAtUtc == null || p.EndsAtUtc >= now))
                    .OrderBy(p => p.SortOrder)
                    .ThenByDescending(p => p.Id)
                    .Select(p => new CatalogPromotionDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        ImageUrl = p.ImageUrl,
                        TargetUrl = p.TargetUrl,
                        SortOrder = p.SortOrder
                    })
                    .ToListAsync(ct);

                // Stats (counts of enabled items)
                var totalCategories = await db.ProductCategories
                    .AsNoTracking()
                    .Where(c => c.IsEnabled && !c.IsDeleted)
                    .CountAsync(ct);

                var totalProducts = await db.Products
                    .AsNoTracking()
                    .Where(p => p.IsEnabled && !p.IsDeleted)
                    .CountAsync(ct);

                return new CatalogHomeDto
                {
                    Categories = categories,
                    NewestProducts = products,
                    Promotions = promotions,
                    Stats = new CatalogHomeStatsDto
                    {
                        TotalCategories = totalCategories,
                        TotalProducts = totalProducts
                    }
                };
            },
            TimeSpan.FromMinutes(5),
            cancellationToken);

        return result!;
    }
}