using Market.Domain.Entities.Dostavljaci;

namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Create;

public class CreateDostavljacCommand : IRequest<int>
{
    public required string Naziv { get; set; }
    public required string Kod { get; set; }
    public required DostavljacTip Tip { get; set; }
    public bool Aktivan { get; set; }=true;
}
