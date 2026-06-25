namespace Market.Application.Modules.Catalog.Dostavljaci.Queries.GetById;

public class GetDostavljacByIdQuery : IRequest<GetDostavljacByIdQueryDto>
{
    public int Id { get; set; }
}