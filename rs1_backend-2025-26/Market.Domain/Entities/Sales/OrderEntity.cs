using Market.Domain.Common;
using Market.Domain.Entities.Identity;

namespace Market.Domain.Entities.Sales;

/// <summary>
/// Represents a customer order in the system.
/// </summary>
public class OrderEntity : BaseEntity
{
    /// <summary>
    /// Reference number (human-readable, e.g., ORD-0001).
    /// </summary>
    public required string ReferenceNumber { get; set; }

    /// <summary>
    /// ID of the user who placed the order.
    /// </summary>
    public required int MarketUserId { get; set; }

    /// <summary>
    /// Associated user entity. (optional)
    /// </summary>
    public MarketUserEntity? MarketUser { get; set; }

    /// <summary>
    /// Date when the order was placed (UTC).
    /// </summary>
    public required DateTime OrderedAtUtc { get; set; }

    /// <summary>
    /// Date when the order was paid (UTC). (optional)
    /// </summary>
    public DateTime? PaidAtUtc { get; set; }

    /// <summary>
    /// Current status of the order.
    /// </summary>
    public required OrderStatusType Status { get; set; }

    /// <summary>
    /// Total amount of the order in the default currency.
    /// </summary>
    public required decimal TotalAmount { get; set; }

    /// <summary>
    /// Additional notes for internal or customer use. (optional)
    /// </summary>
    public string? Note { get; set; }

    ///// <summary>
    ///// Collection of order items.
    /////
    ///// **Napomena za studente:**

    ///// </summary>
    public IReadOnlyCollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();

    /// <summary>
    /// Single source of truth for technical/business constraints.
    /// Used in validators and EF configuration.
    /// </summary>
    public static class Constraints
    {
        public const int ReferenceMaxLength = 10;
    }
}

