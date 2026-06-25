namespace Market.Domain.Exceptions;

public sealed class MarketNotFoundException : Exception
{
    public MarketNotFoundException(string message) : base(message)
    {
    }

    public MarketNotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}
