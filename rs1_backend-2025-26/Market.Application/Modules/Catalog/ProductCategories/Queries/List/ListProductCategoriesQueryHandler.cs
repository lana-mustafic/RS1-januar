namespace Market.Application.Modules.Catalog.ProductCategories.Queries.List;

public sealed class ListProductCategoriesQueryHandler(IAppDbContext ctx)
        : IRequestHandler<ListProductCategoriesQuery, PageResult<ListProductCategoriesQueryDto>>
{
    public async Task<PageResult<ListProductCategoriesQueryDto>> Handle(
        ListProductCategoriesQuery request, CancellationToken ct)
    {
        var q = ctx.ProductCategories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
             q = q.Where(x => x.Name.Contains(request.Search));
        }

        if (request.OnlyEnabled is not null)
            q = q.Where(x => x.IsEnabled == request.OnlyEnabled);

        var projectedQuery = q.OrderBy(x => x.Name)
            .Select(x => new ListProductCategoriesQueryDto
            {
                Id = x.Id,
                Name = x.Name,
                IsEnabled = x.IsEnabled
            });

        return await PageResult<ListProductCategoriesQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
