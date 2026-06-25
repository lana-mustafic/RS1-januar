using Market.Application.Modules.Sales.Orders.Commands.Create;
using Market.Application.Modules.Sales.Orders.Commands.Status;
using Market.Application.Modules.Sales.Orders.Commands.Update;
using Market.Application.Modules.Sales.Orders.Queries.GetById;
using Market.Application.Modules.Sales.Orders.Queries.List;
using Market.Application.Modules.Sales.Orders.Queries.ListWithItems;

namespace Market.API.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    // Fallback policy already requires authenticated user
    public async Task<ActionResult<int>> Create(CreateOrderCommand command, CancellationToken ct)
    {
        int id = await sender.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task Update(int id, UpdateOrderCommand command, CancellationToken ct)
    {
        // ID from the route takes precedence
        command.Id = id;
        await sender.Send(command, ct);
        // no return -> 204 No Content
    }

    [HttpGet("{id:int}")]
    public async Task<GetOrderByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        var dto = await sender.Send(new GetOrderByIdQuery { Id = id }, ct);
        return dto; // if NotFoundException -> 404 via middleware
    }

    [HttpGet]
    public async Task<PageResult<ListOrdersQueryDto>> List([FromQuery] ListOrdersQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result;
    }

    [HttpGet("with-items")]
    public async Task<PageResult<ListOrdersWithItemsQueryDto>> ListWithItems([FromQuery] ListOrdersWithItemsQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result;
    }

    [HttpPut("{id:int}/change-status")]
    [Authorize(Policy = "Staff")]
    public async Task ChangeStatus(int id, [FromBody] ChangeOrderStatusCommand command, CancellationToken ct)
    {
        command.Id = id;
        await sender.Send(command, ct);
        // no return -> 204 No Content
    }
}
