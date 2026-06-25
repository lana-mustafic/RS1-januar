using Market.Application.Abstractions.Caching;

namespace Market.Application.Modules.Catalog.ProductCategories.Commands.Delete;

public class DeleteProductCategoryCommandHandler : IRequestHandler<DeleteProductCategoryCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly IAppCurrentUser _appCurrentUser;
    private readonly ICatalogCacheVersionService _cacheVersionService;

    public DeleteProductCategoryCommandHandler(
        IAppDbContext context,
        IAppCurrentUser appCurrentUser,
        ICatalogCacheVersionService cacheVersionService)
    {
        _context = context;
        _appCurrentUser = appCurrentUser;
        _cacheVersionService = cacheVersionService;
    }

    public async Task<Unit> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        if (_appCurrentUser.UserId is null)
            throw new MarketBusinessRuleException("123", "Korisnik nije autentifikovan.");

        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (category is null)
            throw new MarketNotFoundException("Kategorija nije pronađena.");

        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate catalog cache
        await _cacheVersionService.BumpVersionAsync(cancellationToken);

        return Unit.Value;
    }
}
