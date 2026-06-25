namespace Market.Application.Modules.Catalog.Dostavljaci.Queries.GetById;

public class GetDostavljacByIdQueryHandler(IAppDbContext context) : IRequestHandler<GetDostavljacByIdQuery, GetDostavljacByIdQueryDto>
{
    public async Task<GetDostavljacByIdQueryDto> Handle(GetDostavljacByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await context.Dostavljaci
            .Where(c => c.Id == request.Id)
            .Select(x => new GetDostavljacByIdQueryDto
            {
                Id = x.Id,
                Naziv = x.Naziv,
                Kod = x.Kod,
                Tip=x.Tip,
                Aktivan=x.Aktivan
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            throw new MarketNotFoundException($"Dostavljac sa Id: {request.Id} nije pronađen.");
        }

        return dto;
    }
}