namespace Market.Application.Modules.Fakture.Queries.List;

public sealed class ListFaktureQueryHandler(IAppDbContext ctx)
        : IRequestHandler<ListFaktureQuery, PageResult<ListFaktureQueryDto>>
{
    public async Task<PageResult<ListFaktureQueryDto>> Handle(ListFaktureQuery request, CancellationToken ct)
    {
        var q = ctx.Fakture.AsNoTracking();

        var projectedQuery = q.OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ListFaktureQueryDto
            {
                Id = x.Id,
                BrojRacuna = x.BrojRacuna,
                Tip = x.Tip,
                DatumKreiranja = x.CreatedAtUtc,
                BrojStavki = x.BrojStavki
            });

        return await PageResult<ListFaktureQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
