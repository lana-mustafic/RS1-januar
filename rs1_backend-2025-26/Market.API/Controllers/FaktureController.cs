using Market.Application.Modules.Fakture.Queries.List;

namespace Market.API.Controllers;

[ApiController]
[Route("[controller]")]
public class FaktureController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<PageResult<ListFaktureQueryDto>> List([FromQuery] ListFaktureQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result;
    }
}
