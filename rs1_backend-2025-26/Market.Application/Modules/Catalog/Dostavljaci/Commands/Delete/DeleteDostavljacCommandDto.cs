namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Delete;

public class DeleteDostavljacCommandHandler(IAppDbContext _context, IAppCurrentUser _appCurrentUser) : IRequestHandler<DeleteDostavljacCommand, Unit>
{
    public async Task<Unit> Handle(DeleteDostavljacCommand request, CancellationToken cancellationToken)
    {
        if (_appCurrentUser.UserId is null)
            throw new MarketBusinessRuleException("123", "Korisnik nije autentifikovan.");

        var dto = await _context.Dostavljaci
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (dto is null)
            throw new MarketNotFoundException("Dostavljac nije pronađen.");

        _context.Dostavljaci.Remove(dto);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
