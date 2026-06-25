namespace Market.Application.Modules.Auth.Commands.Logout;

/// <summary>
/// Handler that revokes the user's refresh token (idempotently).
/// </summary>
public sealed class LogoutCommandHandler(IAppDbContext ctx, IJwtTokenService tokens) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken ct)
    {
        var hash = tokens.HashRefreshToken(request.RefreshToken);

        var rt = await ctx.RefreshTokens
            .FirstOrDefaultAsync(x =>
                x.TokenHash == hash &&
                !x.IsRevoked &&
                !x.IsDeleted, ct);

        // Idempotent — if the token does not exist or has already been revoked, nothing happens.
        if (rt is null)
            return;

        rt.IsRevoked = true;
        await ctx.SaveChangesAsync(ct);
    }
}