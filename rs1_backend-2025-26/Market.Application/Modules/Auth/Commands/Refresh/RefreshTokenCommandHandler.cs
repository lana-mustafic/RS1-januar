namespace Market.Application.Modules.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandHandler(
    IAppDbContext ctx,
    IJwtTokenService jwt,
    TimeProvider timeProvider)
    : IRequestHandler<RefreshTokenCommand, RefreshTokenCommandDto>
{
    public async Task<RefreshTokenCommandDto> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        // 1) Hash the received refresh token
        var incomingHash = jwt.HashRefreshToken(request.RefreshToken);

        // 2) Find the valid refresh token in the database (TRACKING because we will modify it)
        var rt = await ctx.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.TokenHash == incomingHash &&
                !x.IsRevoked &&
                !x.IsDeleted, ct);

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        if (rt is null || rt.ExpiresAtUtc <= nowUtc)
            throw new MarketConflictException("Refresh token je nevažeći ili je istekao.");

        // (optional) Fingerprint check
        if (rt.Fingerprint is not null &&
            request.Fingerprint is not null &&
            rt.Fingerprint != request.Fingerprint)
        {
            throw new MarketConflictException("Neispravan klijentski otisak.");
        }

        var user = rt.User;
        if (user is null || !user.IsEnabled || user.IsDeleted)
            throw new MarketConflictException("Korisnički nalog je nevažeći.");

        // 3) Rotation: revoke the old one
        rt.IsRevoked = true;
        rt.RevokedAtUtc = nowUtc;

        // 4) Issue a NEW pair (access + refresh) – the service returns both RAW and HASH along with expirations.
        var pair = jwt.IssueTokens(user);

        // 5) Save the NEW refresh token (HASH only) in the database
        var newRt = new RefreshTokenEntity
        {
            TokenHash = pair.RefreshTokenHash,
            ExpiresAtUtc = pair.RefreshTokenExpiresAtUtc,
            UserId = user.Id,
            Fingerprint = request.Fingerprint,
        };

        ctx.RefreshTokens.Add(newRt);
        await ctx.SaveChangesAsync(ct);

        // 6) Return the RAW refresh token and access token to the client
        return new RefreshTokenCommandDto
        {
            AccessToken = pair.AccessToken,
            RefreshToken = pair.RefreshTokenRaw,
            AccessTokenExpiresAtUtc = pair.AccessTokenExpiresAtUtc,
            RefreshTokenExpiresAtUtc = pair.RefreshTokenExpiresAtUtc
        };
    }
}