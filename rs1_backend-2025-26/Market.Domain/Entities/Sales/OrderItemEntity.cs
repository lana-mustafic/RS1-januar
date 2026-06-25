using Market.Domain.Common;
using Market.Domain.Entities.Catalog;

namespace Market.Domain.Entities.Sales;

/// <summary>
/// Represents a single product line within an order.
/// </summary>
public class OrderItemEntity : BaseEntity
{
    /// <summary>
    /// ID of the parent order.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Associated order. (optional)
    /// </summary>
    public required OrderEntity? Order { get; set; }

    /// <summary>
    /// ID of the product being ordered.
    /// </summary>
    public required int ProductId { get; set; }

    /// <summary>
    /// Associated product. (optional)
    /// </summary>
    public ProductEntity? Product { get; set; }

    /// <summary>
    /// Quantity of the product ordered.
    /// </summary>
    public required decimal Quantity { get; set; }

    /// <summary>
    /// Unit price of the product at the time of order.
    /// </summary>
    public required decimal UnitPrice { get; set; }

    /// <summary>
    /// Calculated subtotal (Quantity × UnitPrice).
    /// </summary>
    public required decimal Subtotal { get; set; }

    /// <summary>
    /// Discount applied to this item (optional).
    /// </summary>
    public required decimal? DiscountAmount { get; set; }

    /// <summary>
    /// Discount percent to this item (optional).
    /// </summary>
    public required decimal? DiscountPercent { get; set; }

    /// <summary>
    /// Final total for this item (Subtotal - Discount).
    /// </summary>
    public required decimal Total { get; set; }
}
