using Market.Domain.Entities.Dostavljaci;

namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Update;

public sealed class UpdateDostavljacCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }
    public required string Naziv { get; set; }
    public required string Kod { get; set; }
    public required DostavljacTip Tip { get; set; }
    public required bool Aktivan { get; set; }
}
