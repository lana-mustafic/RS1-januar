namespace Market.Application.Common.Exceptions;

public sealed class MarketNotFoundException : Exception
{
    public MarketNotFoundException(string message, object? id = null) : base(message) { }
}
