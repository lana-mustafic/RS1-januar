namespace Market.Application.Abstractions.Caching;

public interface ICacheService
{
    Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default) where T : class;
}
