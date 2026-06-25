using Market.Application.Modules.Catalog.CatalogHome.Queries.GetCatalogHome;

namespace Market.API.Controllers;

[ApiController]
[Route("[controller]")]
public class CatalogController(ISender sender) : ControllerBase
{
    [HttpGet("home")]
    [AllowAnonymous]
    public async Task<CatalogHomeDto> GetHome(
        [FromQuery] int? categoryLimit,
        [FromQuery] int? productLimit,
        CancellationToken ct)
    {
        var query = new GetCatalogHomeQuery
        {
            CategoryLimit = categoryLimit ?? 8,
            ProductLimit = productLimit ?? 12
        };

        var result = await sender.Send(query, ct);
        return result;
    }
}
