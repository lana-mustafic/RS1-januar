namespace Market.Domain.Exceptions;

public sealed class MarketConflictException : Exception
{
    public MarketConflictException(string message) : base(message)
    {
    }
}
