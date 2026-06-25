using Market.Application.Abstractions.Caching;

namespace Market.Application.Modules.Catalog.Products.Commands.Delete;

public class DeleteProductCommandHandler(
    IAppDbContext context,
    IAppCurrentUser appCurrentUser,
    ICatalogCacheVersionService cacheVersionService) : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        if (!appCurrentUser.IsAdmin)
            throw new MarketBusinessRuleException("123", "Samo admin moze brisati.");

        var product = await context.Products
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product is null)
            throw new MarketNotFoundException("Proizvod nije pronađena.");

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);

        // Invalidate catalog cache
        await cacheVersionService.BumpVersionAsync(cancellationToken);

        return Unit.Value;
    }
}
