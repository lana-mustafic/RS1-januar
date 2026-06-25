using Market.Application.Abstractions.Caching;

namespace Market.Application.Modules.Catalog.ProductCategories.Commands.Create;

public class CreateProductCategoryCommandHandler(
    IAppDbContext context,
    ICatalogCacheVersionService cacheVersionService) : IRequestHandler<CreateProductCategoryCommand, int>
{
    public async Task<int> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ValidationException("Name is required.");

        // Check if a category with the same name already exists.
        bool exists = await context.ProductCategories
            .AnyAsync(x => x.Name == normalized, cancellationToken);

        if (exists)
        {
            throw new MarketConflictException("Name already exists.");
        }

        var category = new ProductCategoryEntity
        {
            Name = request.Name!.Trim(),
            IsEnabled = request.IsEnabled
        };

        context.ProductCategories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        // Invalidate catalog cache
        await cacheVersionService.BumpVersionAsync(cancellationToken);

        return category.Id;
    }
}
