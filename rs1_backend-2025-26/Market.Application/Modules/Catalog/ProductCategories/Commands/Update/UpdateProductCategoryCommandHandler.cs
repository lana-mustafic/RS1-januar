using Market.Application.Abstractions.Caching;

namespace Market.Application.Modules.Catalog.ProductCategories.Commands.Update;

public sealed class UpdateProductCategoryCommandHandler : IRequestHandler<UpdateProductCategoryCommand, Unit>
{
    private readonly IAppDbContext _ctx;
    private readonly ICatalogCacheVersionService _cacheVersionService;

    public UpdateProductCategoryCommandHandler(
        IAppDbContext ctx,
        ICatalogCacheVersionService cacheVersionService)
    {
        _ctx = ctx;
        _cacheVersionService = cacheVersionService;
    }

    public async Task<Unit> Handle(UpdateProductCategoryCommand request, CancellationToken ct)
    {
        var entity = await _ctx.ProductCategories
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            throw new MarketNotFoundException($"Kategorija (ID={request.Id}) nije pronađena.");

        // Check for duplicate name (case-insensitive, except for the same ID)
        var exists = await _ctx.ProductCategories
            .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == request.Name.ToLower(), ct);

        if (exists)
        {
            throw new MarketConflictException("Name already exists.");
        }

        entity.Name = request.Name.Trim();
        entity.IsEnabled = request.IsEnabled;

        await _ctx.SaveChangesAsync(ct);

        // Invalidate catalog cache
        await _cacheVersionService.BumpVersionAsync(ct);

        return Unit.Value;
    }
}
