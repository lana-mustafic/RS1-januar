// MarketUserEntity.cs
using Market.Domain.Common;
using Market.Domain.Entities.Catalog;

namespace Market.Domain.Entities.Identity;

public sealed class MarketUserEntity : BaseEntity
{
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsManager { get; set; }
    public bool IsEmployee { get; set; }
    public int TokenVersion { get; set; } = 0;// For global revocation
    public bool IsEnabled { get; set; }
    public ICollection<RefreshTokenEntity> RefreshTokens { get; private set; } = new List<RefreshTokenEntity>();

    // Many-to-many: User's favorite products
    public ICollection<UserProductFavoriteEntity> FavoriteProducts { get; private set; } = new List<UserProductFavoriteEntity>();

}