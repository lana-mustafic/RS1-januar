using Market.Domain.Entities.Dostavljaci;

namespace Market.Application.Modules.Catalog.Dostavljaci.Queries.List;

public sealed class ListDostavljacQueryDto
{
    public required int Id { get; init; }
    public required string Naziv { get; init; }
    public required string Kod { get; init; }
    public required DostavljacTip Tip { get; init; }
    public required bool Aktivan { get; init; }
}
