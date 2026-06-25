namespace Market.Domain.Entities.Sales;

/// <summary>
/// Defines possible states of an order.
/// </summary>
public enum OrderStatusType
{
    /// <summary>
    /// Order is created but not yet confirmed.
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Order is confirmed and awaiting payment.
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// Payment received and order is being processed.
    /// </summary>
    Paid = 3,

    /// <summary>
    /// Order has been shipped or delivered.
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Order has been cancelled.
    /// </summary>
    Cancelled = 5
}
