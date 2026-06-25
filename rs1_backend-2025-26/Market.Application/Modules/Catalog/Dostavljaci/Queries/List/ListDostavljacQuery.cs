namespace Market.Application.Modules.Catalog.Dostavljaci.Queries.List;

public sealed class ListDostavljacQuery : BasePagedQuery<ListDostavljacQueryDto>
{
    public string? Search { get; init; }
}
