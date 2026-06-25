using Market.Application.Modules.Catalog.Dostavljaci.Commands.Create;
using Market.Application.Modules.Catalog.Dostavljaci.Commands.Delete;
using Market.Application.Modules.Catalog.Dostavljaci.Commands.Update;
using Market.Application.Modules.Catalog.Dostavljaci.Queries.GetById;
using Market.Application.Modules.Catalog.Dostavljaci.Queries.List;

namespace Market.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = "AdminOnly")]
public class DostavljaciController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateDostavljacCommand command, CancellationToken ct)
    {
        int id = await sender.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task Update(int id, UpdateDostavljacCommand command, CancellationToken ct)
    {
        // ID from the route takes precedence
        command.Id = id;
        await sender.Send(command, ct);
        // no return -> 204 No Content
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteDostavljacCommand { Id = id }, ct);
        // no return -> 204 No Content
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<GetDostavljacByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        var category = await sender.Send(new GetDostavljacByIdQuery { Id = id }, ct);
        return category; // if NotFoundException -> 404 via middleware
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<PageResult<ListDostavljacQueryDto>> List([FromQuery] ListDostavljacQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result;
    }
}
