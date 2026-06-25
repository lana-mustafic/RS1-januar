namespace Market.Application.Common.Exceptions;

/// <summary>
/// Represents an error that occurs when a business rule is violated.
///
/// Such errors do not indicate system issues (e.g., null reference),
/// but situations where a request cannot be executed because it would
/// violate business logic.
///
/// Example: attempting to disable a category that still contains active products.
/// </summary>
public sealed class MarketBusinessRuleException : Exception
{
    public string Code { get; }

    public MarketBusinessRuleException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public MarketBusinessRuleException(string code, string message, Exception? innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}