using Market.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Market.Infrastructure.Caching;

public sealed class CacheService : ICacheService
{
    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default) where T : class
    {
        // No caching - just call factory directly - zbog ispita smo isključiti REDIS
        return await factory(cancellationToken);
    }
}
