namespace Market.Application.Abstractions;

public sealed class JwtTokenPair
{
    public string AccessToken { get; init; }
    public DateTime AccessTokenExpiresAtUtc { get; init; }

    public string RefreshTokenRaw { get; init; }
    public string RefreshTokenHash { get; init; }
    public DateTime RefreshTokenExpiresAtUtc { get; init; }
}

public interface IJwtTokenService
{
    /// <summary>Issues an access token and returns all its technical details.</summary>
    JwtTokenPair IssueTokens(MarketUserEntity user);

    /// <summary>Hashes the refresh token for storage in the database.</summary>
    string HashRefreshToken(string rawToken);
}