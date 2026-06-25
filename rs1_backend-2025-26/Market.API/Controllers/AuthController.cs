using Market.Application.Modules.Auth.Commands.Login;
using Market.Application.Modules.Auth.Commands.Logout;
using Market.Application.Modules.Auth.Commands.Refresh;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginCommandDto>> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        return Ok(await mediator.Send(command, ct));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginCommandDto>> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        return Ok(await mediator.Send(command, ct));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task Logout([FromBody] LogoutCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
    }
}