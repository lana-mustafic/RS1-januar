namespace Market.Application.Modules.Catalog.Dostavljaci.Queries.List;

public sealed class ListDostavljacQueryHandler(IAppDbContext ctx)
        : IRequestHandler<ListDostavljacQuery, PageResult<ListDostavljacQueryDto>>
{
    public async Task<PageResult<ListDostavljacQueryDto>> Handle(
        ListDostavljacQuery request, CancellationToken ct)
    {
        var q = ctx.Dostavljaci.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            q = q.Where(x => x.Naziv.ToLower().Contains(s));
        }


        var projectedQuery = q.OrderBy(x => x.Naziv)
            .Select(x => new ListDostavljacQueryDto
            {
                Id = x.Id,
                Naziv = x.Naziv,
                Kod = x.Kod,
                Tip=x.Tip,
                Aktivan=x.Aktivan
            });

        return await PageResult<ListDostavljacQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
