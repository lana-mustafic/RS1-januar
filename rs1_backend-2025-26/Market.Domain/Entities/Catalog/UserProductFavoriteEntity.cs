using Market.Domain.Common;
using Market.Domain.Entities.Identity;

namespace Market.Domain.Entities.Catalog;

/// <summary>
/// Represents a user's favorite product (many-to-many join table).
/// </summary>
public class UserProductFavoriteEntity : BaseEntity
{
    /// <summary>
    /// Foreign key to the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation to the user who favorited the product.
    /// </summary>
    public MarketUserEntity User { get; set; }

    /// <summary>
    /// Foreign key to the product.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Navigation to the favorited product.
    /// </summary>
    public ProductEntity Product { get; set; }

    /// <summary>
    /// When the product was added to favorites (optional).
    /// </summary>
    public DateTime FavoritedAt { get; set; } = DateTime.UtcNow;
}