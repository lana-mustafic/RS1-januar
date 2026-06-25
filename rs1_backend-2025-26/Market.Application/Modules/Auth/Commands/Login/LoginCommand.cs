namespace Market.Application.Modules.Auth.Commands.Login;

/// <summary>
/// Command for user login and issuing an access/refresh token pair.
/// </summary>
public sealed class LoginCommand : IRequest<LoginCommandDto>
{
    /// <summary>
    /// User's email.
    /// </summary>
    public string Email { get; init; }

    /// <summary>
    /// User's password.
    /// </summary>
    public string Password { get; init; }

    /// <summary>
    /// (Optional) Client "fingerprint" / device identifier for device-bound refresh tokens.
    /// </summary>
    public string? Fingerprint { get; init; }
}