using Market.Domain.Entities.Dostavljaci;

namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Create;

public class CreateDostavljacCommandHandler(
    IAppDbContext context) : IRequestHandler<CreateDostavljacCommand, int>
{
    public async Task<int> Handle(CreateDostavljacCommand request, CancellationToken cancellationToken)
    {
        var naziv = request.Naziv?.Trim();
        var kod = request.Kod?.Trim();

        if (string.IsNullOrWhiteSpace(naziv))
            throw new ValidationException("Polje naziv je obavezno polje.");

        if (string.IsNullOrWhiteSpace(kod))
            throw new ValidationException("Polje kod je obavezno polje.");

        // Check if a category with the same name already exists.
        bool exists = await context.Dostavljaci
            .AnyAsync(x => x.Kod.ToLower() == kod.ToLower(), cancellationToken);

        if (exists)
        {
            throw new MarketConflictException("Ovaj kod vec postoji.");
        }

        var dto = new DostavljacEntity
        {
            Naziv = naziv,
            Kod=kod,
            Tip=request.Tip,
            Aktivan=request.Aktivan
        };

        context.Dostavljaci.Add(dto);
        await context.SaveChangesAsync(cancellationToken);

        return dto.Id;
    }
}
