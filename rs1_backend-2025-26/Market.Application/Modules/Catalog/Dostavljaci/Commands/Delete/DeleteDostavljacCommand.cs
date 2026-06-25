namespace Market.Application.Modules.Catalog.Dostavljaci.Commands.Delete;

public class DeleteDostavljacCommand : IRequest<Unit>
{
    public required int Id { get; set; }
}
