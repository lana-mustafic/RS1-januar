namespace Market.Application.Modules.Catalog.ProductCategories.Queries.GetById;

public class GetProductCategoryByIdQueryHandler(IAppDbContext context) : IRequestHandler<GetProductCategoryByIdQuery, GetProductCategoryByIdQueryDto>
{
    public async Task<GetProductCategoryByIdQueryDto> Handle(GetProductCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await context.ProductCategories
            .Where(c => c.Id == request.Id)
            .Select(x => new GetProductCategoryByIdQueryDto
            {
                Id = x.Id,
                Name = x.Name,
                IsEnabled = x.IsEnabled
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (category == null)
        {
            throw new MarketNotFoundException($"Product category with Id {request.Id} not found.");
        }

        return category;
    }
}