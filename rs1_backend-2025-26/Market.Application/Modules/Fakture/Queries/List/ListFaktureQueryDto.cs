using Market.Domain.Entities.Fakture;

namespace Market.Application.Modules.Fakture.Queries.List;

public sealed class ListFaktureQueryDto
{
    public required int Id { get; init; }
    public required string BrojRacuna { get; init; }
    public required FakturaTip Tip { get; init; }
    public required DateTime DatumKreiranja { get; init; }
    public required int BrojStavki { get; init; }
}
