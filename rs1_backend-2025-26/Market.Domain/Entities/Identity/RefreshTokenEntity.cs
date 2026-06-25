// RefreshTokenEntity.cs

// RefreshTokenEntity.cs
using Market.Domain.Common;

namespace Market.Domain.Entities.Identity;

public sealed class RefreshTokenEntity : BaseEntity
{
    public string TokenHash { get; set; } // Store the HASH, not the plain token
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; }
    public int UserId { get; set; }
    public MarketUserEntity User { get; set; } = default!;
    public string? Fingerprint { get; set; } // (Optional) e.g., UA/IP hash
    public DateTime? RevokedAtUtc { get; set; }
}